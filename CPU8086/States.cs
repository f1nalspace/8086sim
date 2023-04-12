using System;

namespace Final.CPU8086
{
    public enum StateFlag : byte
    {
        Ignore = 0,
        Value,
        Zero,
        One
    }

    public readonly struct States
    {
        public StateFlag Parity { get; }
        public StateFlag Auxiliary { get; }
        public StateFlag Zero { get; }
        public StateFlag Sign { get; }
        public StateFlag Trap { get; }
        public StateFlag Interrupt { get; }
        public StateFlag Direction { get; }
        public StateFlag Overflow { get; }

        public States(StateFlag parity, StateFlag auxiliary, StateFlag zero, StateFlag sign, StateFlag trap, StateFlag interrupt, StateFlag direction, StateFlag overflow)
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

        public States(ReadOnlySpan<char> data)
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

        public static implicit operator States(string value) => new States(value);
        public static explicit operator string(States flags) => flags.ToString();

        private static StateFlag Parse(char c, char t)
        {
            if (c == t)
                return StateFlag.Value;
            return c switch
            {
                '0' => StateFlag.Zero,
                '1' => StateFlag.One,
                '*' or
                '-' => StateFlag.Ignore,
                _ => throw new NotImplementedException($"The state flag char '{c}' is not implemented")
            };
        }

        private static char ToChar(StateFlag flag, char t)
        {
            return flag switch
            {
                StateFlag.Value => t,
                StateFlag.Zero => '0',
                StateFlag.One => '1',
                StateFlag.Ignore => '-',
                _ => throw new NotImplementedException($"The state flag '{flag}' is not implemented")
            };
        }

        public static readonly States Empty = new States();

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
