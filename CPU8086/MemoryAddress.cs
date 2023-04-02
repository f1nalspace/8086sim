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

        public string GetAssembly(DataWidthType dataWidth, OutputValueMode outputMode = OutputValueMode.AsHex, string hexPrefix = "0x")
        {
            int d = Displacement;
            string append;
            if (d == 0)
                append = string.Empty;
            else
            {
                byte displacementLength = EffectiveAddressCalculationTable.Instance.GetDisplacementLength(EAC);

                char op = '+';
                if (displacementLength == 1)
                {
                    if ((sbyte)d < 0)
                    {
                        op = '-';
                        d = Math.Abs((sbyte)d);
                    }
                }
                else
                {
                    if (d < 0)
                    {
                        op = '-';
                        d = Math.Abs(d);
                    }
                }
                string address = Immediate.GetValueAssembly(dataWidth, ImmediateType.Word, d, outputMode, hexPrefix);
                append = $" {op} {address}";
            }
            return EAC switch
            {
                EffectiveAddressCalculation.BX_SI => "[bx + si]",
                EffectiveAddressCalculation.BX_DI => "[bx + di]",
                EffectiveAddressCalculation.BP_SI => "[bp + si]",
                EffectiveAddressCalculation.BP_DI => "[bp + di]",
                EffectiveAddressCalculation.SI => "[si]",
                EffectiveAddressCalculation.DI => "[di]",
                EffectiveAddressCalculation.DirectAddress => $"[{Immediate.GetValueAssembly(dataWidth, ImmediateType.Word, d, outputMode, hexPrefix)}]",
                EffectiveAddressCalculation.BX => "[bx]",
                EffectiveAddressCalculation.BX_SI_D8 => $"[bx + si{append}]",
                EffectiveAddressCalculation.BX_DI_D8 => $"[bx + di{append}]",
                EffectiveAddressCalculation.BP_SI_D8 => $"[bp + si{append}]",
                EffectiveAddressCalculation.BP_DI_D8 => $"[bp + di{append}]",
                EffectiveAddressCalculation.SI_D8 => $"[si{append}]",
                EffectiveAddressCalculation.DI_D8 => $"[di{append}]",
                EffectiveAddressCalculation.BP_D8 => $"[bp{append}]",
                EffectiveAddressCalculation.BX_D8 => $"[bx{append}]",
                EffectiveAddressCalculation.BX_SI_D16 => $"[bx + si{append}]",
                EffectiveAddressCalculation.BX_DI_D16 => $"[bx + di{append}]",
                EffectiveAddressCalculation.BP_SI_D16 => $"[bp + si{append}]",
                EffectiveAddressCalculation.BP_DI_D16 => $"[bp + di{append}]",
                EffectiveAddressCalculation.SI_D16 => $"[si{append}]",
                EffectiveAddressCalculation.DI_D16 => $"[di{append}]",
                EffectiveAddressCalculation.BP_D16 => $"[bp{append}]",
                EffectiveAddressCalculation.BX_D16 => $"[bx{append}]",
                _ => throw new NotImplementedException($"Not supported effective address calculation of '{EAC}'"),
            };
        }

        public override string ToString() => $"{EAC}: {Displacement:X}";
    }
}
