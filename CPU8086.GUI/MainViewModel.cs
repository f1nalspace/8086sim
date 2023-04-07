using DevExpress.Mvvm;
using OneOf;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Final.CPU8086
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly InstructionStreamResources _resources = new InstructionStreamResources();

        private readonly CPU _cpu;

        public string[] ResourceStreams { get; }

        public string CurrentResource { get => _currentResource; set => SetValue(ref _currentResource, value, () => LoadStreamFromResource(value)); }
        private string _currentResource = null;

        public ImmutableArray<byte> CurrentStream { get => _currentStream; private set => SetValue(ref _currentStream, value, () => CurrentStreamChanged(value)); }
        private ImmutableArray<byte> _currentStream = ImmutableArray<byte>.Empty;

        public ImmutableArray<Instruction> Instructions { get => _instructions; private set => SetValue(ref _instructions, value); }
        private ImmutableArray<Instruction> _instructions = ImmutableArray<Instruction>.Empty;

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

        public bool ShowStreamAsHex { get => GetValue<bool>(); set => SetValue(value); }
        public bool ShowRegisterAsHex { get => GetValue<bool>(); set => SetValue(value); }

        public CPURegister Register => _cpu.Register;

        public DelegateCommand ResetCommand { get; }

        public MainViewModel()
        {
            ResetCommand = new DelegateCommand(Reset);

            _cpu = new CPU();
            _cpu.PropertyChanged += OnCPUPropertyChanged;

            ShowStreamAsHex = true;
            ShowRegisterAsHex = true;

            var names = _resources.GetNames();
            ResourceStreams = names.Where(n => string.IsNullOrEmpty(Path.GetExtension(n))).ToArray();

            _currentResource = ResourceStreams[0];
            Stream stream = _resources.Get(CurrentResource);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            CurrentStream = data.ToImmutableArray();
        }

        private void OnCPUPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(CPU.Register).Equals(e.PropertyName))
                RaisePropertyChanged(nameof(Register));
        }

        public void LoadStreamFromResource(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && _resources.Get(name) is Stream stream)
            {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                CurrentStream = data.ToImmutableArray();

            }
            else
                CurrentStream = ImmutableArray<byte>.Empty;
        }

        private void Reset()
        {
            Reset(CurrentStream);
        }

        private void Reset(ImmutableArray<byte> stream)
        {
            if (stream.Length > 0)
                CurrentStreamPosition = 0;
            else
                CurrentStreamPosition = -1;
            _cpu.Reset();
        }

        private void DecodeInstructions(ImmutableArray<byte> stream)
        {
            List<Instruction> list = new List<Instruction>();
            ReadOnlySpan<byte> cur = stream.AsSpan();
            int position = 0;
            while (cur.Length > 0)
            {
                OneOf<Instruction, Error> r = _cpu.TryDecodeNext(cur, CurrentResource, position);
                if (r.IsT1)
                    break;
                Instruction instruction = r.AsT0;
                list.Add(instruction);
                cur = cur.Slice(instruction.Length);
                position += instruction.Length;
            }
            Instructions = list.ToImmutableArray();
        }

        public void CurrentStreamChanged(ImmutableArray<byte> stream)
        {
            Reset(stream);
            DecodeInstructions(stream);
        }
    }
}
