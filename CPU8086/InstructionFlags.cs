using System;
using System.Security.Claims;

namespace Final.CPU8086
{
    public enum InstructionFlag : byte
    {
        Ignore = 0,
        Value,
        Zero,
        One
    }

    public readonly struct InstructionFlags
    {
        public InstructionFlag Parity { get; }
        public InstructionFlag Auxiliary { get; }
        public InstructionFlag Zero { get; }
        public InstructionFlag Sign { get; }
        public InstructionFlag Trap { get; }
        public InstructionFlag Interrupt { get; }
        public InstructionFlag Direction { get; }
        public InstructionFlag Overflow { get; }

        public InstructionFlags(InstructionFlag parity, InstructionFlag auxiliary, InstructionFlag zero, InstructionFlag sign, InstructionFlag trap, InstructionFlag interrupt, InstructionFlag direction, InstructionFlag overflow)
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

        public InstructionFlags(ReadOnlySpan<char> data)
        {
            if (data.Length != 8)
                throw new ArgumentException($"Expect the instruction flags to be a length of 8, but got {data.Length}");
            Parity = Parse(data[7], 'p');
            Auxiliary = Parse(data[6], 'a');
            Zero = Parse(data[5], 'z');
            Sign = Parse(data[4], 's');
            Trap = Parse(data[3], 't');
            Interrupt = Parse(data[2], 'i');
            Direction = Parse(data[1], 'd');
            Overflow = Parse(data[0], 'o');
        }

        private static InstructionFlag Parse(char c, char t)
        {
            if (c == t)
                return InstructionFlag.Value;
            return c switch
            {
                '0' => InstructionFlag.Zero,
                '1' => InstructionFlag.One,
                '*' or
                '-' => InstructionFlag.Ignore,
                _ => throw new NotImplementedException($"The instruction flag char '{c}' is not implemented")
            };
        }

        private static char ToChar(InstructionFlag flag, char t)
        {
            return flag switch
            {
                InstructionFlag.Value => t,
                InstructionFlag.Zero => '0',
                InstructionFlag.One => '1',
                InstructionFlag.Ignore => '-',
                _ => throw new NotImplementedException($"The instruction flag '{flag}' is not implemented")
            };
        }

        public override string ToString()
        {
            Span<char> s = stackalloc char[9];
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
