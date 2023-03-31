using System;
using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct MemoryAddress : IEquatable<MemoryAddress>
    {
        public EffectiveAddressCalculation EAC { get; }
        public int Displacement { get; }

        public MemoryAddress(EffectiveAddressCalculation eac, int displacement)
        {
            EAC = eac;
            Displacement = displacement;
        }

        public bool Equals(MemoryAddress other)
        {
            if (EAC != other.EAC)
                return false;
            if (Displacement != other.Displacement)
                return false;
            return true;
        }
        public override bool Equals(object obj) => obj is MemoryAddress mem && Equals(mem);
        public override int GetHashCode() => HashCode.Combine(EAC, Displacement);

        public override string ToString() => $"{EAC}: {Displacement:X}";
    }
}
