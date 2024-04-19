using System;
using System.Runtime.InteropServices;

namespace Final.CPU8086.Types
{
    public enum FlagDefinitionValue : byte
    {
        Ignore = 0,
        Value,
        Zero,
        One
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct FlagsDefinition
    {
        public FlagDefinitionValue Carry { get; }
        public FlagDefinitionValue Parity { get; }
        public FlagDefinitionValue Auxiliary { get; }
        public FlagDefinitionValue Zero { get; }
        public FlagDefinitionValue Sign { get; }
        public FlagDefinitionValue Trap { get; }
        public FlagDefinitionValue Interrupt { get; }
        public FlagDefinitionValue Direction { get; }
        public FlagDefinitionValue Overflow { get; }

        public FlagsDefinition(FlagDefinitionValue carry, FlagDefinitionValue parity, FlagDefinitionValue auxiliary, FlagDefinitionValue zero, FlagDefinitionValue sign, FlagDefinitionValue trap, FlagDefinitionValue interrupt, FlagDefinitionValue direction, FlagDefinitionValue overflow)
        {
            Carry = carry;
            Parity = parity;
            Auxiliary = auxiliary;
            Zero = zero;
            Sign = sign;
            Trap = trap;
            Interrupt = interrupt;
            Direction = direction;
            Overflow = overflow;
        }

        public FlagsDefinition(FlagDefinitionValue parity, FlagDefinitionValue auxiliary, FlagDefinitionValue zero, FlagDefinitionValue sign, FlagDefinitionValue trap, FlagDefinitionValue interrupt, FlagDefinitionValue direction, FlagDefinitionValue overflow) :
            this(FlagDefinitionValue.Ignore, parity, auxiliary, zero, sign, trap, interrupt, direction, overflow)
        { }

        public FlagsDefinition(ushort status)
        {
            Carry = (status & RegisterState.CarryFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Parity = (status & RegisterState.ParityFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Auxiliary = (status & RegisterState.AuxiliaryCarryFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Zero = (status & RegisterState.ZeroFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Sign = (status & RegisterState.SignFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Trap = (status & RegisterState.TrapFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Interrupt = (status & RegisterState.InterruptFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Direction = (status & RegisterState.DirectionalFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
            Overflow = (status & RegisterState.OverflowFlagMask) != 0 ? FlagDefinitionValue.One : FlagDefinitionValue.Zero;
        }

        public FlagsDefinition(ReadOnlySpan<char> data)
        {
            if (data.Length < 8)
                throw new ArgumentException($"Expect the state flags to be at least length of 8, but got {data.Length}");
            Carry = (data.Length == 9) ? Parse(data[8], 'c') : FlagDefinitionValue.Ignore;
            Parity = Parse(data[7], 'p');
            Auxiliary = Parse(data[6], 'a');
            Zero = Parse(data[5], 'z');
            Sign = Parse(data[4], 's');
            Trap = Parse(data[3], 't');
            Interrupt = Parse(data[2], 'i');
            Direction = Parse(data[1], 'd');
            Overflow = Parse(data[0], 'o');
        }

        public static implicit operator FlagsDefinition(string value) => new FlagsDefinition(value);
        public static explicit operator string(FlagsDefinition flags) => flags.ToString();

        private static FlagDefinitionValue Parse(char c, char t)
        {
            if (c == t)
                return FlagDefinitionValue.Value;
            return c switch
            {
                '0' => FlagDefinitionValue.Zero,
                '1' => FlagDefinitionValue.One,
                '*' or
                '-' => FlagDefinitionValue.Ignore,
                _ => throw new NotImplementedException($"The state flag char '{c}' is not implemented")
            };
        }

        private static char ToChar(FlagDefinitionValue flag, char t)
        {
            return flag switch
            {
                FlagDefinitionValue.Value => t,
                FlagDefinitionValue.Zero => '0',
                FlagDefinitionValue.One => '1',
                FlagDefinitionValue.Ignore => '-',
                _ => throw new NotImplementedException($"The state flag '{flag}' is not implemented")
            };
        }

        public static readonly FlagsDefinition Empty = new FlagsDefinition();

        public override string ToString()
        {
            Span<char> s = stackalloc char[9];
            s[8] = ToChar(Carry, 'c');
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
