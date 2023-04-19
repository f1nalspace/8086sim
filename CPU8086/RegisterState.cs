using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    public class RegisterState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void RaisePropertiesChanged(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
                RaisePropertyChanged(propertyName);
        }

        private void SetValue<T>(ref T reference, T value, Action callback = null, [CallerMemberName] string propertyName = null)
        {
            T oldValue = reference;
            if (object.Equals(oldValue, value))
                return;
            reference = value;
            RaisePropertyChanged(propertyName);
            if (callback != null)
                callback.Invoke();
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        struct R16
        {
            [FieldOffset(0)]
            public short X;
            [FieldOffset(0)]
            public sbyte L;
            [FieldOffset(1)]
            public sbyte H;
        }

        private R16 _ax = new R16();
        public short AX { get => _ax.X; set => SetValue(ref _ax.X, value, () => RaisePropertiesChanged(nameof(AL), nameof(AH))); }
        public sbyte AL { get => _ax.L; set => SetValue(ref _ax.L, value, () => RaisePropertyChanged(nameof(AX))); }
        public sbyte AH { get => _ax.H; set => SetValue(ref _ax.H, value, () => RaisePropertyChanged(nameof(AX))); }

        private R16 _bx = new R16();
        public short BX { get => _bx.X; set => SetValue(ref _bx.X, value, () => RaisePropertiesChanged(nameof(BL), nameof(BH))); }
        public sbyte BL { get => _bx.L; set => SetValue(ref _bx.L, value, () => RaisePropertyChanged(nameof(BX))); }
        public sbyte BH { get => _bx.H; set => SetValue(ref _bx.H, value, () => RaisePropertyChanged(nameof(BX))); }

        private R16 _cx = new R16();
        public short CX { get => _cx.X; set => SetValue(ref _cx.X, value, () => RaisePropertiesChanged(nameof(CL), nameof(CH))); }
        public sbyte CL { get => _cx.L; set => SetValue(ref _cx.L, value, () => RaisePropertyChanged(nameof(CX))); }
        public sbyte CH { get => _cx.H; set => SetValue(ref _cx.H, value, () => RaisePropertyChanged(nameof(CX))); }

        private R16 _dx = new R16();
        public short DX { get => _dx.X; set => SetValue(ref _dx.X, value, () => RaisePropertiesChanged(nameof(DL), nameof(DH))); }
        public sbyte DL { get => _dx.L; set => SetValue(ref _dx.L, value, () => RaisePropertyChanged(nameof(DX))); }
        public sbyte DH { get => _dx.H; set => SetValue(ref _dx.H, value, () => RaisePropertyChanged(nameof(DX))); }

        private short _SP = 0;
        private short _BP = 0;
        private short _SI = 0;
        private short _DI = 0;
        public short SP { get => _SP; set => SetValue(ref _SP, value); }
        public short BP { get => _BP; set => SetValue(ref _BP, value); }
        public short SI { get => _SI; set => SetValue(ref _SI, value); }
        public short DI { get => _DI; set => SetValue(ref _DI, value); }

        private short _CS = 0;
        private short _DS = 0;
        private short _SS = 0;
        private short _ES = 0;
        public short CS { get => _CS; set => SetValue(ref _CS, value); }
        public short DS { get => _DS; set => SetValue(ref _DS, value); }
        public short SS { get => _SS; set => SetValue(ref _SS, value); }
        public short ES { get => _ES; set => SetValue(ref _ES, value); }

        private ushort _IP = 0;
        public ushort IP { get => _IP; set => SetValue(ref _IP, value); }

        private ushort _Status = 0;
        public ushort Status { get => _Status; set => SetValue(ref _Status, value); }

        public const byte ZeroFlagBit = 6;
        public const ushort ZeroFlagMask = 1 << ZeroFlagBit;

        public bool ZeroFlag
        {
            get => (Status & ZeroFlagMask) == ZeroFlagMask;
            set
            {
                if (value)
                    Status = (ushort)(Status & ~ZeroFlagMask);
                else
                    Status = (ushort)(Status | ZeroFlagMask);
            } 
        }

        public void Reset()
        {
            AX = 0;
            BX = 0;
            CX = 0;
            DX = 0;

            SP = 0;
            BP = 0;
            SI = 0;
            DI = 0;

            CS = 0;
            DS = 0;
            SS = 0;
            ES = 0;

            IP = 0;

            Status = 0;
        }

        public void Assign(RegisterState register)
        {
            if (register == null)
                throw new ArgumentNullException(nameof(register));
            AX = register.AX;
            BX = register.BX;
            CX = register.CX;
            DX = register.DX;

            SP = register.SP;
            BP = register.BP;
            SI = register.SI;
            DI = register.DI;

            CS = register.CS;
            DS = register.DS;
            SS = register.SS;
            ES = register.ES;

            IP = register.IP;

            Status = register.Status;
        }
    }
}
