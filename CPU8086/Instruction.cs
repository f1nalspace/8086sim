namespace Final.CPU8086
{
    public readonly struct Instruction
    {
        public InstructionType Type { get; }
        public DataWidth Width { get; }
        public byte Length { get; }
        public byte OpCode { get; } // Only for debug
        public InstructionOperand Dest { get; }
        public InstructionOperand Source { get; }

        public Instruction(InstructionType type, DataWidth width, byte length, byte opCode, InstructionOperand dest, InstructionOperand source)
        {
            Type = type;
            Width = width;
            Length = length;
            OpCode = opCode;
            Dest = dest;
            Source = source;
        }

        public Instruction(InstructionType type, DataWidth dataType, byte length, byte opCode, InstructionOperand dest) : this(type, dataType, length, opCode, dest, new InstructionOperand()) { }

        public Instruction(InstructionType type, DataWidth dataType, byte length, byte opCode) : this(type, dataType, length, opCode, new InstructionOperand(), new InstructionOperand()) { }

        public override string ToString()
        {
            if (Dest.Type != OperandType.None && Source.Type != OperandType.None)
                return $"{Type} {Dest}, {Source} (op: {OpCode:X2}, {Length} bytes)";
            else if (Dest.Type != OperandType.None && Source.Type == OperandType.None)
                return $"{Type} {Dest} (op: {OpCode:X2}, {Length} bytes)";
            else
                return $"{Type} (op: {OpCode:X2}, {Length} bytes)";
        }
    }
}
