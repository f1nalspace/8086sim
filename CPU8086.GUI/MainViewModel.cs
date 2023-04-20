using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using Final.CPU8086.Execution;
using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
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

        public ImmutableArray<Instruction> Instructions { get => _instructions; private set => SetValue(ref _instructions, value, () => InstructionsChanged(value)); }
        private ImmutableArray<Instruction> _instructions = ImmutableArray<Instruction>.Empty;
        private ImmutableArray<uint> _instructionIndexMap = ImmutableArray<uint>.Empty;

        public ImmutableArray<AssemblyLine> AssemblyLines { get => _assemblyLines; private set => SetValue(ref _assemblyLines, value); }
        private ImmutableArray<AssemblyLine> _assemblyLines = ImmutableArray<AssemblyLine>.Empty;

        public ObservableCollection<Error> Errors { get; }

        public ObservableCollection<LogItemViewModel> Logs { get; }

        public uint CurrentStreamPosition
        {
            get => _currentStreamPosition;
            private set
            {
                _currentStreamPosition = value;
                RaisePropertyChanged(nameof(CurrentStreamPosition));
            }
        }
        private uint _currentStreamPosition = uint.MaxValue;

        public bool ShowStreamAsHex { get => _showStreamAsHex; set => SetValue(ref _showStreamAsHex, value); }
        private bool _showStreamAsHex = true;

        public bool ShowRegisterAsHex { get => _showRegisterAsHex; set => SetValue(ref _showRegisterAsHex, value); }
        private bool _showRegisterAsHex = true;

        public bool ShowAssemblyAsHex { get => _showAssemblyAsHex; set => SetValue(ref _showAssemblyAsHex, value, () => ShowAssemblyAsHexChanged(value)); }
        private bool _showAssemblyAsHex = true;

        public bool ShowMemoryAsHex { get => _showMemoryAsHex; set => SetValue(ref _showMemoryAsHex, value); }
        private bool _showMemoryAsHex = true;

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

        public RegisterState Register => _cpu.Register;

        public ImmutableArray<byte> MemoryPage { get => GetValue<ImmutableArray<byte>>(); private set => SetValue(value); }
        public int MemoryPageIndex { get => GetValue<int>(); set => SetValue(value, () => MemoryPageIndexChanged(value)); }
        public int MemoryPageOffset { get => GetValue<int>(); private set => SetValue(value); }

        public int SelectedStreamOrMemoryTabIndex { get => GetValue<int>(); set => SetValue(value); }

        public bool CanChangeStream => ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Failed || ExecutionState == ExecutionState.Halted;

        public DelegateCommand RunCommand { get; }
        public DelegateCommand StopCommand { get; }
        public DelegateCommand StepCommand { get; }
        public DelegateCommand JumpToFirstMemoryPageCommand { get; }
        public DelegateCommand JumpToLastMemoryPageCommand { get; }

        private volatile Task _executionTask = null;
        private volatile int _isStopping = 0;

        public MainViewModel()
        {
            _cpu = new CPU();
            _cpu.PropertyChanged += OnCPUPropertyChanged;

            RunCommand = new DelegateCommand(Run, CanRun);
            StopCommand = new DelegateCommand(Stop, CanStop);
            StepCommand = new DelegateCommand(Step, CanStep);
            JumpToFirstMemoryPageCommand = new DelegateCommand(JumpToFirstMemoryPage, CanJumpToFirstMemoryPage);
            JumpToLastMemoryPageCommand = new DelegateCommand(JumpToLastMemoryPage, CanJumpToLastMemoryPage);

            Errors = new ObservableCollection<Error>();
            Logs = new ObservableCollection<LogItemViewModel>();

            _executionTask = null;

            CurrentInstruction = null;
            CurrentProgram = null;
            CurrentStream = ImmutableArray<byte>.Empty;
            DecodeState = DecodeState.None;
            ExecutionState = ExecutionState.Stopped;
            SelectedStreamOrMemoryTabIndex = 0;

            string[] resNames = _resources.GetNames();

            Programs = resNames
                .Where(n => string.IsNullOrEmpty(Path.GetExtension(n)))
                .Select(n => new Program(n, _resources.Get(n)))
                .ToArray();

            CurrentProgram = Programs[0];

            MemoryPage = _cpu.Memory.ReadPage(MemoryPageIndex);
            MemoryPageIndex = 0;
            MemoryPageOffset = MemoryPageIndex * MemoryTable.PageSize;
        }

        private void AddLog(uint position, string message)
        {
            if (_dispatcherService != null)
                _dispatcherService.Invoke(() => Logs.Add(new LogItemViewModel(position, message, DateTimeOffset.Now)));
            else
                Logs.Add(new LogItemViewModel(position, message, DateTimeOffset.Now));
        }

        private bool CanJumpToFirstMemoryPage() => MemoryPageIndex > 0;
        private void JumpToFirstMemoryPage()
        {
            SelectedStreamOrMemoryTabIndex = 1;
            MemoryPageIndex = 0;
        }

        private bool CanJumpToLastMemoryPage() => MemoryPageIndex < (_cpu.Memory.PageCount - 1);
        private void JumpToLastMemoryPage()
        {
            SelectedStreamOrMemoryTabIndex = 1;
            MemoryPageIndex = _cpu.Memory.PageCount - 1;
        }

        private void MemoryPageIndexChanged(int index)
        {
            MemoryPageOffset = index * MemoryTable.PageSize;
            JumpToFirstMemoryPageCommand.RaiseCanExecuteChanged();
            JumpToLastMemoryPageCommand.RaiseCanExecuteChanged();
        }


        private void OnCPUPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(CPU.Register).Equals(e.PropertyName))
                RaisePropertyChanged(nameof(Register));
            else if (nameof(CPU.Memory).Equals(e.PropertyName))
                MemoryPage = _cpu.Memory.ReadPage(MemoryPageIndex);
        }

        private void InstructionsChanged(IEnumerable<Instruction> instructions)
        {
            RefreshAssemblyLines(instructions, ShowAssemblyAsHex);
        }

        private void ShowAssemblyAsHexChanged(bool asHex)
        {
            RefreshAssemblyLines(Instructions, asHex);
        }

        public void LoadProgram(IProgram program)
        {
            AddLog(0, $"Loading program '{program}'");

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
            _isStopping = 0;
            ExecutionState = ExecutionState.Running;

            AddLog(CurrentStreamPosition, $"Running program '{CurrentProgram}' with '{CurrentInstruction.Length}' instructions");

            _cpu.Reset();

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

                    AddLog(CurrentInstruction.Position, $"Execute instruction '{CurrentInstruction}'");
                    OneOf<int, Error> r = _cpu.ExecuteInstruction(CurrentInstruction);
                    if (r.IsT1)
                    {
                        ExecutionState = ExecutionState.Failed;
                        _dispatcherService.Invoke(() => Errors.Add(r.AsT1));
                        break;
                    }

                    int relativePos = r.AsT0;
                    uint nextPos = (uint)(CurrentStreamPosition + relativePos);

                    // Stream is finished
                    if (nextPos >= CurrentStream.Length)
                    {
                        isFinished = true;
                        break;
                    }

                    uint nextInstructionIndex = _instructionIndexMap[(int)nextPos];
                    if (nextInstructionIndex == uint.MaxValue)
                        throw new InvalidDataException($"No instruction mapped to stream position '{nextInstructionIndex}'!");

                    CurrentStreamPosition = nextPos;
                    CurrentInstruction = Instructions[(int)nextInstructionIndex];
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
                CurrentStreamPosition = uint.MaxValue;
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
                    CurrentStreamPosition = uint.MaxValue;
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
            AddLog(CurrentInstruction.Position, $"Stopping program '{CurrentProgram}'");
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
                CurrentStreamPosition = uint.MaxValue;
                _isStopping = 0;
                RefreshCommands();
            }
        }

        private bool CanStep() =>
            Instructions.Length > 0 &&
            CurrentStream.Length > 0 &&
            (CurrentStreamPosition == uint.MaxValue || CurrentStreamPosition < CurrentStream.Length) &&
            DecodeState == DecodeState.Success &&
            (ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Halted) &&
            (_executionTask == null || _executionTask.IsCompleted);
        private void Step()
        {
            Contract.Assert(CanStep());
            if (ExecutionState == ExecutionState.Stopped)
            {
                AddLog(CurrentStreamPosition, $"Start stepping into program '{CurrentProgram}' with '{Instructions.Length}' instructions");
                CurrentInstruction = Instructions.First();
                CurrentStreamPosition = CurrentInstruction.Position;
                Interlocked.Exchange(ref _isStopping, 0);
                ExecutionState = ExecutionState.Halted;
            }
            else if (ExecutionState == ExecutionState.Halted)
            {
                AddLog(CurrentStreamPosition, $"Continue stepping into program '{CurrentProgram}' with '{Instructions.Length}' instructions");
                Interlocked.Exchange(ref _isStopping, 0);
                ExecutionState = ExecutionState.Running;
                _executionTask = Task.Run(() => ExecuteAsync(true));
            }
        }

        private void RefreshAssemblyLines(IEnumerable<Instruction> instructions, bool asHex)
        {
            OneOf<AssemblyLine[], Error> res = _cpu.GetAssemblyLines(instructions, asHex ? OutputValueMode.AsHex : OutputValueMode.AsInteger);
            if (res.IsT0)
                AssemblyLines = res.AsT0.ToImmutableArray();
            else
                AssemblyLines = ImmutableArray<AssemblyLine>.Empty;
        }

        private void DecodeInstructions(IProgram program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            CurrentStreamPosition = uint.MaxValue;
            CurrentInstruction = null;
            ExecutionState = ExecutionState.Stopped;
            DecodeState = DecodeState.Decoding;

            AddLog(CurrentStreamPosition, $"Decoding instructions for program '{program}'");

            List<Instruction> list = new List<Instruction>();

            ImmutableArray<byte> stream = program.Stream;

            uint[] instructionIndexMap = new uint[stream.Length];
            for (int i = 0; i < stream.Length; ++i)
                instructionIndexMap[i] = uint.MaxValue;

            Errors.Clear();
            try
            {
                ReadOnlySpan<byte> cur = stream.AsSpan();
                uint position = 0;
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
                    instructionIndexMap[position] = (uint)list.Count;
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
