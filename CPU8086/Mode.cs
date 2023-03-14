namespace Final.CPU8086
{
    public enum Mode : byte
    {
        MemoryNoDisplacement = 0b00,
        MemoryByteDisplacement = 0b01,
        MemoryWordDisplacement = 0b10,
        RegisterMode = 0b11,
    }
}
