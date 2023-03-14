namespace Final.CPU8086
{
    public enum ArithmeticType : byte
    {
        Add = 0b000,
        Or = 0b001,
        AddWithCarry = 0b010,
        SubWithBorrow = 0b011,
        And = 0b100,
        Sub = 0b101,
        Xor = 0b110,
        Compare = 0b111,
    }
}
