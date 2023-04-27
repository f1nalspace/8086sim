using System;
using System.Runtime.InteropServices;

namespace Final.CPU8086.Types
{
    public enum FlagValue : byte
    {
        Ignore = 0,
        Value,
        Zero,
        One
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Flags
    {
        public FlagValue Parity { get; }
        public FlagValue Auxiliary { get; }
        public FlagValue Zero { get; }
        public FlagValue Sign { get; }
        public FlagValue Trap { get; }
        public FlagValue Interrupt { get; }
        public FlagValue Direction { get; }
        public FlagValue Overflow { get; }

        public Flags(FlagValue parity, FlagValue auxiliary, FlagValue zero, FlagValue sign, FlagValue trap, FlagValue interrupt, FlagValue direction, FlagValue overflow)
        {
            Parity = parity;
            Auxiliary = auxiliary;
            Zero = zero;
            Sign = sign;
            Trap = trap;
            Interrupt = interrupt;
            Direction = direction;
            Overflow = overflow;
        }

        public Flags(ReadOnlySpan<char> data)
        {
            if (data.Length != 8)
                throw new ArgumentException($"Expect the state flags to be a length of 8, but got {data.Length}");
            Parity = Parse(data[7], 'p');
            Auxiliary = Parse(data[6], 'a');
            Zero = Parse(data[5], 'z');
            Sign = Parse(data[4], 's');
            Trap = Parse(data[3], 't');
            Interrupt = Parse(data[2], 'i');
            Direction = Parse(data[1], 'd');
            Overflow = Parse(data[0], 'o');
        }

        public static implicit operator Flags(string value) => new Flags(value);
        public static explicit operator string(Flags flags) => flags.ToString();

        private static FlagValue Parse(char c, char t)
        {
            if (c == t)
                return FlagValue.Value;
            return c switch
            {
                '0' => FlagValue.Zero,
                '1' => FlagValue.One,
                '*' or
                '-' => FlagValue.Ignore,
                _ => throw new NotImplementedException($"The state flag char '{c}' is not implemented")
            };
        }

        private static char ToChar(FlagValue flag, char t)
        {
            return flag switch
            {
                FlagValue.Value => t,
                FlagValue.Zero => '0',
                FlagValue.One => '1',
                FlagValue.Ignore => '-',
                _ => throw new NotImplementedException($"The state flag '{flag}' is not implemented")
            };
        }

        public static readonly Flags Empty = new Flags();

        public override string ToString()
        {
            Span<char> s = stackalloc char[8];
            s[7] = ToChar(Parity, 'p');
            s[6] = ToChar(Auxiliary, 'a');
            s[5] = ToChar(Zero, 'z');
            s[4] = ToChar(Sign, 's');
            s[3] = ToChar(Trap, 't');
            s[2] = ToChar(Interrupt, 'i');
            s[1] = ToChar(Direction, 'd');
            s[0] = ToChar(Overflow, 'o');
            return s.ToString();
        }
    }
}
