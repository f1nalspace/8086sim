namespace Final.CPU8086
{
    public enum OperandType : byte
    {
        None = 0,
        Register,
        Address,
        Immediate,
        Value,
    }
}
