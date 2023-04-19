namespace Final.CPU8086
{
    public readonly struct StreamByte
    {
        public uint Index { get; }
        public byte Value { get; }

        public StreamByte(uint index, byte value)
        {
            Index = index;
            Value = value;
        }
    }
}
