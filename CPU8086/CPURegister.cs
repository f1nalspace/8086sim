using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public class CPURegister
    {
        [FieldOffset(0)]
        private ushort _AX;
        [FieldOffset(0)]
        private byte _AL;
        [FieldOffset(1)]
        private byte _AH;

        public ushort AX { get => _AX; set => _AX = value; }
        public byte AL { get => _AL; set => _AL = value; }
        public byte AH { get => _AH; set => _AH = value; }

        [FieldOffset(2)]
        private ushort _BX;
        [FieldOffset(2)]
        private byte _BL;
        [FieldOffset(3)]
        private byte _BH;

        public ushort BX { get => _BX; set => _BX = value; }
        public byte BL { get => _BL; set => _BL = value; }
        public byte BH { get => _BH; set => _BH = value; }

        [FieldOffset(4)]
        private ushort _CX;
        [FieldOffset(4)]
        private byte _CL;
        [FieldOffset(5)]
        private byte _CH;

        public ushort CX { get => _CX; set => _CX = value; }
        public byte CL { get => _CL; set => _CL = value; }
        public byte CH { get => _CH; set => _CH = value; }

        [FieldOffset(6)]
        private ushort _DX;
        [FieldOffset(6)]
        private byte _DL;
        [FieldOffset(7)]
        private byte _DH;

        public ushort DX { get => _DX; set => _DX = value; }
        public byte DL { get => _DL; set => _DL = value; }
        public byte DH { get => _DH; set => _DH = value; }

        [FieldOffset(8)]
        private ushort _SP;
        [FieldOffset(10)]
        private ushort _BP;
        [FieldOffset(12)]
        private ushort _SI;
        [FieldOffset(14)]
        private ushort _DI;

        public ushort SP { get => _SP; set => _SP = value; }
        public ushort BP { get => _BP; set => _BP = value; }
        public ushort SI { get => _SI; set => _SI = value; }
        public ushort DI { get => _DI; set => _DI = value; }

        [FieldOffset(16)]
        private ushort _CS;
        [FieldOffset(18)]
        private ushort _DS;
        [FieldOffset(20)]
        private ushort _SS;
        [FieldOffset(22)]
        private ushort _ES;

        public ushort CS { get => _CS; set => _CS = value; }
        public ushort DS { get => _DS; set => _DS = value; }
        public ushort SS { get => _SS; set => _SS = value; }
        public ushort ES { get => _ES; set => _ES = value; }

        [FieldOffset(24)]
        private ushort _IP;

        public ushort IP { get => _IP; set => _IP = value; }

        [FieldOffset(26)]
        private ushort _Status;

        public ushort Status { get => _Status; set => _Status = value; }
    }
}
