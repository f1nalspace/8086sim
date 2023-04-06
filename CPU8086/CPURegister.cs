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

        [FieldOffset(16)]
        private ushort _CS;
        [FieldOffset(18)]
        private ushort _DS;
        [FieldOffset(20)]
        private ushort _SS;
        [FieldOffset(22)]
        private ushort _ES;

        [FieldOffset(24)]
        private ushort _IP;

        [FieldOffset(26)]
        private ushort _Status;
        [FieldOffset(26)]
        private ushort _StatusLow;
        [FieldOffset(27)]
        private ushort _StatusHigh;
    }
}
