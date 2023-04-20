namespace Final.CPU8086.Instructions
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
