using System;
using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public class RegisterState
    {
        [FieldOffset(0)]
        private short _AX;
        [FieldOffset(0)]
        private sbyte _AL;
        [FieldOffset(1)]
        private sbyte _AH;

        public short AX { get => _AX; set => _AX = value; }
        public sbyte AL { get => _AL; set => _AL = value; }
        public sbyte AH { get => _AH; set => _AH = value; }

        [FieldOffset(2)]
        private short _BX;
        [FieldOffset(2)]
        private sbyte _BL;
        [FieldOffset(3)]
        private sbyte _BH;

        public short BX { get => _BX; set => _BX = value; }
        public sbyte BL { get => _BL; set => _BL = value; }
        public sbyte BH { get => _BH; set => _BH = value; }

        [FieldOffset(4)]
        private short _CX;
        [FieldOffset(4)]
        private sbyte _CL;
        [FieldOffset(5)]
        private sbyte _CH;

        public short CX { get => _CX; set => _CX = value; }
        public sbyte CL { get => _CL; set => _CL = value; }
        public sbyte CH { get => _CH; set => _CH = value; }

        [FieldOffset(6)]
        private short _DX;
        [FieldOffset(6)]
        private sbyte _DL;
        [FieldOffset(7)]
        private sbyte _DH;

        public short DX { get => _DX; set => _DX = value; }
        public sbyte DL { get => _DL; set => _DL = value; }
        public sbyte DH { get => _DH; set => _DH = value; }

        [FieldOffset(8)]
        private short _SP;
        [FieldOffset(10)]
        private short _BP;
        [FieldOffset(12)]
        private short _SI;
        [FieldOffset(14)]
        private short _DI;

        public short SP { get => _SP; set => _SP = value; }
        public short BP { get => _BP; set => _BP = value; }
        public short SI { get => _SI; set => _SI = value; }
        public short DI { get => _DI; set => _DI = value; }

        [FieldOffset(16)]
        private short _CS;
        [FieldOffset(18)]
        private short _DS;
        [FieldOffset(20)]
        private short _SS;
        [FieldOffset(22)]
        private short _ES;

        public short CS { get => _CS; set => _CS = value; }
        public short DS { get => _DS; set => _DS = value; }
        public short SS { get => _SS; set => _SS = value; }
        public short ES { get => _ES; set => _ES = value; }

        [FieldOffset(24)]
        private ushort _IP;
        public ushort IP { get => _IP; set => _IP = value; }

        [FieldOffset(26)]
        private ushort _Status;
        public ushort Status { get => _Status; set => _Status = value; }

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
