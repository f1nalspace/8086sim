using System;
using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    public enum ImmediateType : byte
    {
        None = 0,
        Byte,
        SignedByte,
        Word,
        SignedWord,
    }

    [Flags]
    public enum ImmediateFlag : byte
    {
        None = 0,
        RelativeJumpDisplacement = 1 << 0,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct Immediate : IEquatable<Immediate>
    {
        [FieldOffset(0)]
        public readonly ImmediateType Type;
        [FieldOffset(1)]
        public readonly ImmediateFlag Flags;
        [FieldOffset(2)]
        public readonly byte U8;
        [FieldOffset(2)]
        public readonly sbyte S8;
        [FieldOffset(2)]
        public readonly ushort U16;
        [FieldOffset(2)]
        public readonly short S16;
        [FieldOffset(2)]
        public readonly uint U32;
        [FieldOffset(2)]
        public readonly int S32;

        public Immediate(byte u8, ImmediateFlag flags) : this()
        {
            Type = ImmediateType.Byte;
            Flags = flags;
            U8 = u8;
        }

        public Immediate(sbyte s8, ImmediateFlag flags) : this()
        {
            Type = ImmediateType.SignedByte;
            Flags = flags;
            S8 = s8;
        }

        public Immediate(ushort u16, ImmediateFlag flags) : this()
        {
            Type = ImmediateType.Word;
            Flags = flags;
            U16 = u16;
        }

        public Immediate(short s16, ImmediateFlag flags) : this()
        {
            Type = ImmediateType.SignedWord;
            Flags = flags;
            S16 = s16;
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
        public override int GetHashCode() => HashCode.Combine(Type, Flags, S32);

        public static string GetValueAssembly(ImmediateType type, int value, OutputValueMode outputMode = OutputValueMode.AsHexAuto)
        {
            int v = type switch
            {
                ImmediateType.Byte => (byte)value,
                ImmediateType.SignedByte => (sbyte)value,
                ImmediateType.Word => (ushort)value,
                ImmediateType.SignedWord => (short)value,
                _ => value,
            };
            return outputMode switch
            {
                OutputValueMode.AsHexAuto => $"0x{v:X}",
                OutputValueMode.AsHex8 => $"0x{v:X2}",
                OutputValueMode.AsHex16 => $"0x{v:X4}",
                OutputValueMode.AsHex32 => $"0x{v:X8}",
                _ => v.ToString(),
            };
        }

        public string GetAssembly(OutputValueMode outputMode = OutputValueMode.AsHexAuto)
            => GetValueAssembly(Type, S32, outputMode);

        public override string ToString()
        {
            return Type switch
            {
                ImmediateType.Byte => U8.ToString("X2"),
                ImmediateType.SignedByte => S8.ToString("G"),
                ImmediateType.Word => U16.ToString("X4"),
                ImmediateType.SignedWord => S16.ToString("G"),
                _ => string.Empty,
            };
        }
    }
}
