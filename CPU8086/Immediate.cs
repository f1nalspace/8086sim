namespace Final.CPU8086
{
    public readonly struct Immediate
    {
        public short Value { get; }
        public ImmediateFlag Flags { get; }

        public Immediate(short value, ImmediateFlag flags) : this()
        {
            Value = value;
            Flags = flags;
        }

        public override string ToString() => Value.ToString("X4");
    }
}
