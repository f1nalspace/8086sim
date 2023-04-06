using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Native;
using Final.CPU8086;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Documents;

namespace Final.CPU8086
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly InstructionStreamResources _resources = new InstructionStreamResources();

        private readonly CPU _cpu;

        public string[] ResourceStreams { get; }

        public string CurrentResource { get => _currentResource; set => SetValue(ref _currentResource, value, () => CurrentResourceChanged(value)); }
        private string _currentResource = null;

        public ImmutableArray<StreamByte> CurrentStream { get => _currentStream; private set => SetValue(ref _currentStream, value, () => CurrentStreamChanged(value)); }
        private ImmutableArray<StreamByte> _currentStream = ImmutableArray<StreamByte>.Empty;

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

        public MainViewModel()
        {
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

            int index = 0;
            _currentStream = data
                .Select(d => new StreamByte(Interlocked.Add(ref index, 1) - 1, d))
                .ToImmutableArray();

            _currentStreamPosition = 0;
        }

        private void OnCPUPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(CPU.Register).Equals(e.PropertyName))
                RaisePropertyChanged(nameof(Register));
        }

        public void CurrentResourceChanged(string newName)
        {
            Stream stream = _resources.Get(newName);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            int index = 0;
            CurrentStream = data
                .Select(d => new StreamByte(Interlocked.Add(ref index, 1), d))
                .ToImmutableArray();
            CurrentStreamPosition = 0;
        }

        public void CurrentStreamChanged(ImmutableArray<StreamByte> newStream)
        {

        }
    }
}
