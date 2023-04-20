namespace Final.CPU8086.Types
{
    public enum ModType : byte
    {
        MemoryNoDisplacement = 0b00,
        MemoryByteDisplacement = 0b01,
        MemoryWordDisplacement = 0b10,
        RegisterMode = 0b11,
        Unknown = 0xFF
    }
}
