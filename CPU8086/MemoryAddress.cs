using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct MemoryAddress
    {
        public EffectiveAddressCalculation EAC { get; }
        public int Displacement { get; }

        public MemoryAddress(EffectiveAddressCalculation eac, short displacement)
        {
            EAC = eac;
            Displacement = displacement;
        }

        public override string ToString() => $"{EAC}: {Displacement:X4}";
    }
}
