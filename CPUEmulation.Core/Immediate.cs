using System;
using System.Runtime.InteropServices;

namespace Final.CPUEmulation
{
    public enum ImmediateType : byte
    {
        None = 0,
        U8,
        S8,
        U16,
        S16,
        U32,
        S32
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1,  Size = 8)]
    public readonly struct Immediate : IEquatable<Immediate>
    {
        const byte ByteIntThreshold = byte.MaxValue / 2;
        const ushort WordIntThreshold = ushort.MaxValue / 2;
        const uint DoubleWordThreshold = uint.MaxValue / 4;

        [FieldOffset(0)]
        public readonly ImmediateType Type;
        [FieldOffset(1)]
        public readonly byte Flags;
        [FieldOffset(2)]
        public readonly short Padding;
        [FieldOffset(4)]
        public readonly byte U8;
        [FieldOffset(4)]
        public readonly sbyte S8;
        [FieldOffset(4)]
        public readonly ushort U16;
        [FieldOffset(4)]
        public readonly short S16;
        [FieldOffset(4)]
        public readonly uint U32;
        [FieldOffset(4)]
        public readonly int S32;

        public int Value
        {
            get
            {
                return Type switch
                {
                    ImmediateType.U8 => U8,
                    ImmediateType.S8 => S8,
                    ImmediateType.U16 => U16,
                    ImmediateType.S16 => S16,
                    ImmediateType.U32 => (int)U32,
                    ImmediateType.S32 => S32,
                    _ => 0,
                };
            }
        }

        public Immediate(byte u8, byte flags = 0) : this()
        {
            Type = ImmediateType.U8;
            Flags = flags;
            U8 = u8;
        }

        public Immediate(sbyte s8, byte flags = 0) : this()
        {
            Type = ImmediateType.S8;
            Flags = flags;
            S8 = s8;
        }

        public Immediate(ushort u16, byte flags = 0) : this()
        {
            Type = ImmediateType.U16;
            Flags = flags;
            U16 = u16;
        }

        public Immediate(short s16, byte flags = 0) : this()
        {
            Type = ImmediateType.S16;
            Flags = flags;
            S16 = s16;
        }

        public Immediate(uint u32, byte flags = 0) : this()
        {
            Type = ImmediateType.U32;
            Flags = flags;
            U32 = u32;
        }

        public Immediate(int s32, byte flags = 0) : this()
        {
            Type = ImmediateType.S32;
            Flags = flags;
            S32 = s32;
        }

        public bool Equals(Immediate other)
        {
            if (Type != other.Type)
                return false;
            if (Flags != other.Flags)
                return false;
            if (S32 != other.S32)
                return false;
            return true;
        }
        public override bool Equals(object obj) => obj is Immediate imm && Equals(imm);
        public static bool operator ==(Immediate left, Immediate right) => left.Equals(right);
        public static bool operator !=(Immediate left, Immediate right) => !left.Equals(right);
        public override int GetHashCode() => HashCode.Combine(Type, Flags, S32);

        public static string IntToHex(int value, DataWidthType dataWidth, int digits)
        {
            string result;
            if (digits > 0)
                result = value.ToString($"X{digits}");
            else
                result = value.ToString("X");
            switch (dataWidth)
            {
                case DataWidthType.Byte:
                    if (result.Length > 2)
                        return result.Substring(result.Length - 2);
                    break;
                case DataWidthType.Word:
                    if (result.Length > 4)
                        return result.Substring(result.Length - 4);
                    break;
                case DataWidthType.DoubleWord:
                    if (result.Length > 8)
                        return result.Substring(result.Length - 8);
                    break;
                case DataWidthType.QuadWord:
                    if (result.Length > 16)
                        return result.Substring(result.Length - 16);
                    break;
                default:
                    break;
            }
            return result;
        }

        public static string GetValueAssembly(DataWidthType dataWidth, ImmediateType type, int value, OutputValueMode outputMode = OutputValueMode.Auto, string hexPrefix = "0x")
        {
            int v = value;
            switch (type)
            {
                case ImmediateType.U8:
                    if (outputMode == OutputValueMode.Auto)
                    {
                        if (value <= ByteIntThreshold)
                            return value.ToString("D");
                        else
                            return $"{hexPrefix}{IntToHex((byte)value, dataWidth, 2)}";
                    }
                    else
                        v = (byte)(value & 0xFF);
                    break;
                case ImmediateType.S8:
                    if (outputMode == OutputValueMode.Auto)
                        return $"{(sbyte)value:D}";
                    else
                        v = (sbyte)(value & 0xFF);
                    break;
                case ImmediateType.U16:
                    if (outputMode == OutputValueMode.Auto)
                    {
                        if (value <= WordIntThreshold)
                            return $"{(ushort)value:D}";
                        else
                            return $"{hexPrefix}{IntToHex((ushort)value, dataWidth, 4)}";
                    }
                    else
                        v = (ushort)(value & 0xFFFF);
                    break;
                case ImmediateType.S16:
                    if (outputMode == OutputValueMode.Auto)
                        return $"{(short)value:D}";
                    else
                        v = (short)(value & 0xFFFF);
                    break;
                case ImmediateType.U32:
                    if (value <= DoubleWordThreshold)
                        return value.ToString("D");
                    else
                        return $"{hexPrefix}{IntToHex((int)(uint)value, dataWidth, 8)}";
                case ImmediateType.S32:
                    if (outputMode == OutputValueMode.Auto)
                        return $"{value:D}";
                    else
                        v = (int)(value & 0xFFFFFFFF);
                    break;
                default:
                    break;
            }
            if (outputMode == OutputValueMode.AsHex || outputMode == OutputValueMode.Auto)
            {
                switch (dataWidth)
                {
                    case DataWidthType.Byte:
                        outputMode = OutputValueMode.AsHex8;
                        break;
                    case DataWidthType.Word:
                        outputMode = OutputValueMode.AsHex16;
                        break;
                    case DataWidthType.DoubleWord:
                        outputMode = OutputValueMode.AsHex32;
                        break;
                    case DataWidthType.QuadWord:
                        outputMode = OutputValueMode.AsHex64;
                        break;
                    default:
                        break;
                }
            }
            return outputMode switch
            {
                OutputValueMode.AsHex => $"{hexPrefix}{IntToHex(v, dataWidth, -1)}",
                OutputValueMode.AsHex8 => $"{hexPrefix}{IntToHex(v, dataWidth, 2)}",
                OutputValueMode.AsHex16 => $"{hexPrefix}{IntToHex(v, dataWidth, 4)}",
                OutputValueMode.AsHex32 => $"{hexPrefix}{IntToHex(v, dataWidth, 8)}",
                OutputValueMode.AsHex64 => $"{hexPrefix}{IntToHex(v, dataWidth, 16)}",
                _ => v.ToString(),
            };
        }

        public string Asm(DataWidthType dataWidth, OutputValueMode outputMode = OutputValueMode.Auto, string hexPrefix = "0x")
            => GetValueAssembly(dataWidth, Type, S32, outputMode, hexPrefix);

        public override string ToString()
        {
            return Type switch
            {
                ImmediateType.U8 => $"0x{U8:X2}",
                ImmediateType.S8 => S8.ToString("G"),
                ImmediateType.U16 => $"0x{U16:X4}",
                ImmediateType.S16 => S16.ToString("G"),
                ImmediateType.U32 => $"0x{U32:X8}",
                ImmediateType.S32 => S32.ToString("G"),
                _ => string.Empty,
            };
        }

        
    }
}
