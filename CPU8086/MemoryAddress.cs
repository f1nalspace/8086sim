using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public readonly struct MemoryAddress
    {
        public EffectiveAddressCalculation EAC { get; }
        public byte Padding { get; }
        public short Displacement { get; }

        public MemoryAddress(EffectiveAddressCalculation eac, short displacement)
        {
            Padding = 0;
            EAC = eac;
            Displacement = displacement;
        }

        public override string ToString() => $"{EAC}: {Displacement:X4}";
    }
}
