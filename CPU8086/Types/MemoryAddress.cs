using System;
using System.Runtime.InteropServices;

namespace Final.CPU8086.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct MemoryAddress : IEquatable<MemoryAddress>
    {
        public EffectiveAddressCalculation EAC { get; }
        public SegmentType SegmentType { get; }
        public int Displacement { get; }
        public uint SegmentAddress { get; }

        public MemoryAddress(EffectiveAddressCalculation eac, int displacement, SegmentType segmentType, uint segmentAddress)
        {
            EAC = eac;
            Displacement = displacement;
            SegmentType = segmentType;
            SegmentAddress = segmentAddress;
        }

        public bool Equals(MemoryAddress other)
        {
            if (EAC != other.EAC)
                return false;
            if (Displacement != other.Displacement)
                return false;
            if (SegmentType != other.SegmentType)
                return false;
            if (SegmentAddress != other.SegmentAddress)
                return false;
            return true;
        }
        public override bool Equals(object obj) => obj is MemoryAddress mem && Equals(mem);
        public override int GetHashCode() => HashCode.Combine(EAC, Displacement, SegmentType, SegmentAddress);
        public static bool operator ==(MemoryAddress left, MemoryAddress right) => left.Equals(right);
        public static bool operator !=(MemoryAddress left, MemoryAddress right) => !(left == right);

        public string Asm(DataWidthType dataWidth, OutputValueMode outputMode = OutputValueMode.AsHex, string hexPrefix = "0x")
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
                    if ((short)d < 0)
                    {
                        op = '-';
                        d = Math.Abs((short)d);
                    }
                    else if (d < 0)
                    {
                        op = '-';
                        d = Math.Abs(d);
                    }
                }
                string address = Immediate.GetValueAssembly(dataWidth, ImmediateType.Word, d, outputMode, hexPrefix);
                append = $" {op} {address}";
            }

            string segmentPrefix;
            switch (SegmentType)
            {
                case SegmentType.CS:
                    segmentPrefix = "cs:";
                    break;
                case SegmentType.DS:
                    segmentPrefix = "ds:";
                    break;
                case SegmentType.SS:
                    segmentPrefix = "ss:";
                    break;
                case SegmentType.ES:
                    segmentPrefix = "es:";
                    break;
                default:
                    if (SegmentAddress > 0)
                        segmentPrefix = $"{Immediate.GetValueAssembly(dataWidth, ImmediateType.Word, (int)SegmentAddress, outputMode, hexPrefix)}:";
                    else
                        segmentPrefix = string.Empty;
                    break;
            }

            return EAC switch
            {
                EffectiveAddressCalculation.BX_SI => $"[{segmentPrefix}bx + si]",
                EffectiveAddressCalculation.BX_DI => $"[{segmentPrefix}bx + di]",
                EffectiveAddressCalculation.BP_SI => $"[{segmentPrefix}bp + si]",
                EffectiveAddressCalculation.BP_DI => $"[{segmentPrefix}bp + di]",
                EffectiveAddressCalculation.SI => $"[{segmentPrefix}si]",
                EffectiveAddressCalculation.DI => $"[{segmentPrefix}di]",
                EffectiveAddressCalculation.DirectAddress => $"[{segmentPrefix}{Immediate.GetValueAssembly(dataWidth, ImmediateType.Word, d, outputMode, hexPrefix)}]",
                EffectiveAddressCalculation.BX => $"[{segmentPrefix}bx]",
                EffectiveAddressCalculation.BX_SI_D8 => $"[{segmentPrefix}bx + si{append}]",
                EffectiveAddressCalculation.BX_DI_D8 => $"[{segmentPrefix}bx + di{append}]",
                EffectiveAddressCalculation.BP_SI_D8 => $"[{segmentPrefix}bp + si{append}]",
                EffectiveAddressCalculation.BP_DI_D8 => $"[{segmentPrefix}bp + di{append}]",
                EffectiveAddressCalculation.SI_D8 => $"[{segmentPrefix}si{append}]",
                EffectiveAddressCalculation.DI_D8 => $"[{segmentPrefix}di{append}]",
                EffectiveAddressCalculation.BP_D8 => $"[{segmentPrefix}bp{append}]",
                EffectiveAddressCalculation.BX_D8 => $"[{segmentPrefix}bx{append}]",
                EffectiveAddressCalculation.BX_SI_D16 => $"[{segmentPrefix}bx + si{append}]",
                EffectiveAddressCalculation.BX_DI_D16 => $"[{segmentPrefix}bx + di{append}]",
                EffectiveAddressCalculation.BP_SI_D16 => $"[{segmentPrefix}bp + si{append}]",
                EffectiveAddressCalculation.BP_DI_D16 => $"[{segmentPrefix}bp + di{append}]",
                EffectiveAddressCalculation.SI_D16 => $"[{segmentPrefix}si{append}]",
                EffectiveAddressCalculation.DI_D16 => $"[{segmentPrefix}di{append}]",
                EffectiveAddressCalculation.BP_D16 => $"[{segmentPrefix}bp{append}]",
                EffectiveAddressCalculation.BX_D16 => $"[{segmentPrefix}bx{append}]",
                _ => throw new NotImplementedException($"Not supported effective address calculation of '{EAC}'"),
            };
        }

        public override string ToString() => $"EAC: {EAC}, Disp: 0x{Displacement:X}, Seg: {SegmentType}:0x{SegmentAddress:X}";
    }
}
