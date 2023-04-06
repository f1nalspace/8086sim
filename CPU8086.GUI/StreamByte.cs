namespace Final.CPU8086
{
    public readonly struct StreamByte
    {
        public int Index { get; }
        public byte Value { get; }

        public StreamByte(int index, byte value)
        {
            Index = index;
            Value = value;
        }
    }
}
