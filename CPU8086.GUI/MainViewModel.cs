using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using OneOf;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Final.CPU8086
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly InstructionStreamResources _resources = new InstructionStreamResources();

        private IDispatcherService _dispatcherService => GetService<IDispatcherService>();

        private readonly CPU _cpu;

        public IProgram[] Programs { get; }

        public IProgram CurrentProgram { get => GetValue<IProgram>(); set => SetValue(value, () => LoadProgram(value)); }

        public ImmutableArray<byte> CurrentStream { get => GetValue<ImmutableArray<byte>>(); private set => SetValue(value); }

        public ImmutableArray<Instruction> Instructions { get => _instructions; private set => SetValue(ref _instructions, value); }
        private ImmutableArray<Instruction> _instructions = ImmutableArray<Instruction>.Empty;
        private ImmutableArray<int> _instructionIndexMap = ImmutableArray<int>.Empty;

        public ObservableCollection<Error> Errors { get; }

        public int CurrentStreamPosition
        {
            get => _currentStreamPosition;
            private set
            {
                _currentStreamPosition = value;
                RaisePropertyChanged(nameof(CurrentStreamPosition));
            }
        }
        private int _currentStreamPosition = -1;

        public bool ShowStreamAsHex { get => _showStreamAsHex; set => SetValue(ref _showStreamAsHex, value); }
        private bool _showStreamAsHex = true;

        public bool ShowRegisterAsHex { get => _showRegisterAsHex; set => SetValue(ref _showRegisterAsHex, value); }
        private bool _showRegisterAsHex = true;

        public bool ShowAssemblyAsHex { get => _showAssemblyAsHex; set => SetValue(ref _showAssemblyAsHex, value); }
        private bool _showAssemblyAsHex = true;

        public ExecutionState ExecutionState
        {
            get => _executionState;
            private set
            {
                _executionState = value;
                RaisePropertyChanged(nameof(ExecutionState));
                RefreshCommands();
            }
        }
        private volatile ExecutionState _executionState = ExecutionState.Stopped;

        public DecodeState DecodeState
        {
            get => _decodeState;
            private set
            {
                _decodeState = value;
                RaisePropertyChanged(nameof(DecodeState));
                RefreshCommands();
            }
        }
        private volatile DecodeState _decodeState = DecodeState.None;

        public Instruction CurrentInstruction { get => GetValue<Instruction>(); private set => SetValue(value); }

        public CPURegister Register => _cpu.Register;

        public bool CanChangeStream => ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Failed || ExecutionState == ExecutionState.Halted;

        public DelegateCommand RunCommand { get; }
        public DelegateCommand StopCommand { get; }
        public DelegateCommand StepCommand { get; }

        private volatile Task _executionTask = null;
        private volatile int _isStopping = 0;

        public MainViewModel()
        {
            _cpu = new CPU();
            _cpu.PropertyChanged += OnCPUPropertyChanged;

            RunCommand = new DelegateCommand(Run, CanRun);
            StopCommand = new DelegateCommand(Stop, CanStop);
            StepCommand = new DelegateCommand(Step, CanStep);

            Errors = new ObservableCollection<Error>();

            _executionTask = null;

            CurrentInstruction = null;
            CurrentProgram = null;
            CurrentStream = ImmutableArray<byte>.Empty;
            DecodeState = DecodeState.None;
            ExecutionState = ExecutionState.Stopped;

            string[] resNames = _resources.GetNames();

            Programs = resNames
                .Where(n => string.IsNullOrEmpty(Path.GetExtension(n)))
                .Select(n => new Program(n, _resources.Get(n)))
                .ToArray();

            CurrentProgram = Programs[0];
        }

        private void OnCPUPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(CPU.Register).Equals(e.PropertyName))
                RaisePropertyChanged(nameof(Register));
        }

        public void LoadProgram(IProgram program)
        {
            if (CanStop())
                Stop();

            if (program != null)
            {
                CurrentStream = program.Stream;
                DecodeInstructions(program);
            }
            else
                CurrentStream = ImmutableArray<byte>.Empty;
        }

        private void RefreshCommands()
        {
            _dispatcherService?.Invoke(() =>
            {
                RunCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
                StepCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(CanChangeStream));
            });
        }

        private bool CanRun() =>
            DecodeState == DecodeState.Success &&
            (ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Failed) &&
            (_executionTask == null || _executionTask.IsCompleted);
        private void Run()
        {
            Contract.Assert(CanRun());
            CurrentInstruction = Instructions.First();
            CurrentStreamPosition = CurrentInstruction.Position;
            _cpu.Reset();
            _isStopping = 0;
            ExecutionState = ExecutionState.Running;
            _executionTask = Task.Run(() => ExecuteAsync(false));
        }

        private async Task ExecuteAsync(bool isSingleStep)
        {
            _dispatcherService.Invoke(() => Errors.Clear());
            bool isFinished = false;
            try
            {
                while (CurrentInstruction != null && ExecutionState == ExecutionState.Running)
                {
                    if (_isStopping == 1)
                    {
                        ExecutionState = ExecutionState.Stopped;
                        break;
                    }

                    await Task.Delay(10);

                    OneOf<int, Error> r = _cpu.ExecuteInstruction(CurrentInstruction);
                    if (r.IsT1)
                    {
                        ExecutionState = ExecutionState.Failed;
                        _dispatcherService.Invoke(() => Errors.Add(r.AsT1));
                        break;
                    }

                    int relativePos = r.AsT0;
                    int nextPos = CurrentStreamPosition + relativePos;

                    // Stream is finished
                    if (nextPos >= CurrentStream.Length)
                    {
                        isFinished = true;
                        break;
                    }

                    int nextInstructionIndex = _instructionIndexMap[nextPos];
                    if (nextInstructionIndex == -1)
                        throw new InvalidDataException($"No instruction mapped to stream position '{nextInstructionIndex}'!");

                    CurrentStreamPosition = nextPos;
                    CurrentInstruction = Instructions[nextInstructionIndex];
                    if (isSingleStep)
                        break;
                }
            }
            catch (Exception e)
            {
                _dispatcherService.Invoke(() => Errors.Add(new Error(ErrorCode.ExecutionFailed, $"Failed to execute instruction '{CurrentInstruction}' in position '{CurrentStreamPosition}': {e}", CurrentStreamPosition)));
                ExecutionState = ExecutionState.Failed;
            }

            Interlocked.Exchange(ref _isStopping, 0);

            if (isFinished)
            {
                CurrentInstruction = null;
                CurrentStreamPosition = -1;
                ExecutionState = ExecutionState.Stopped;
            }
            else if (ExecutionState != ExecutionState.Failed)
            {
                if (isSingleStep && CurrentInstruction != null)
                {
                    ExecutionState = ExecutionState.Halted;
                }
                else
                {
                    CurrentInstruction = null;
                    CurrentStreamPosition = -1;
                    ExecutionState = ExecutionState.Stopped;
                }
            }

            RefreshCommands();
        }

        private bool CanStop() =>
            DecodeState == DecodeState.Success &&
            (ExecutionState == ExecutionState.Running || ExecutionState == ExecutionState.Halted);
        private async void Stop()
        {
            Contract.Assert(CanStop());
            await StopAsync();
        }

        private async Task StopAsync()
        {
            Interlocked.Exchange(ref _isStopping, 1);
            try
            {
                while (Interlocked.CompareExchange(ref _isStopping, 0, 0) == 0)
                {
                    if (ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Failed)
                        break;
                    await Task.Delay(50);
                }
            }
            finally
            {
                ExecutionState = ExecutionState.Stopped;
                CurrentInstruction = null;
                CurrentStreamPosition = -1;
                _isStopping = 0;
                RefreshCommands();
            }
        }

        private bool CanStep() =>
            Instructions.Length > 0 &&
            CurrentStream.Length > 0 &&
            (CurrentStreamPosition == -1 || CurrentStreamPosition < CurrentStream.Length) &&
            DecodeState == DecodeState.Success &&
            (ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Halted) &&
            (_executionTask == null || _executionTask.IsCompleted);
        private void Step()
        {
            Contract.Assert(CanStep());
            if (ExecutionState == ExecutionState.Stopped)
            {
                CurrentInstruction = Instructions.First();
                CurrentStreamPosition = CurrentInstruction.Position;
                Interlocked.Exchange(ref _isStopping, 0);
                ExecutionState = ExecutionState.Halted;
            }
            else if (ExecutionState == ExecutionState.Halted)
            {
                Interlocked.Exchange(ref _isStopping, 0);
                ExecutionState = ExecutionState.Running;
                _executionTask = Task.Run(() => ExecuteAsync(true));
            }
            
            
        }

        private void DecodeInstructions(IProgram program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            Contract.Assert(ExecutionState == ExecutionState.Stopped);
            Contract.Assert(CurrentInstruction == null);
            Contract.Assert(CurrentStreamPosition == -1);

            DecodeState = DecodeState.Decoding;

            List<Error> errors = new List<Error>();
            List<Instruction> list = new List<Instruction>();

            ImmutableArray<byte> stream = program.Stream;

            int[] instructionIndexMap = new int[stream.Length];
            for (int i = 0; i < stream.Length; ++i)
                instructionIndexMap[i] = -1;

            Errors.Clear();
            try
            {
                ReadOnlySpan<byte> cur = stream.AsSpan();
                int position = 0;
                while (cur.Length > 0)
                {
                    OneOf<Instruction, Error> r = _cpu.TryDecodeNext(cur, program.Name, position);
                    if (r.IsT1)
                    {
                        var err = new Error(r.AsT1, $"Failed to decode instruction stream '{program}'", position);
                        Errors.Add(err);
                        break;
                    }
                    Instruction instruction = r.AsT0;
                    instructionIndexMap[position] = list.Count;
                    list.Add(instruction);
                    cur = cur.Slice(instruction.Length);
                    position += instruction.Length;
                }
                Instructions = list.ToImmutableArray();
                _instructionIndexMap = instructionIndexMap.ToImmutableArray();
            }
            finally
            {
                DecodeState = (Errors.Any() || stream.Length == 0) ? DecodeState.Failed : DecodeState.Success;
            }
        }
    }
}
