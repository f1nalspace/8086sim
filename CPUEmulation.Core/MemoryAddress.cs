namespace Final.CPUEmulation
{
    public readonly struct MemoryAddress
    {
        public uint Segment { get; }
        public uint Displacement { get; }
        public uint Flags { get; }
        public uint Expression { get; }

        public MemoryAddress(uint segment, uint displacement, uint flags = 0, uint expression = 0)
        {
            Segment = segment;
            Displacement = displacement;
            Flags = flags;
            Expression = expression;
        }
    }
}
