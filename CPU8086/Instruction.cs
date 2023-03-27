using System;
using System.Text;

namespace Final.CPU8086
{
    public readonly struct Instruction
    {
        public byte OpCode { get; }
        public byte Length { get; }
        public InstructionType Type { get; }
        public DataWidth Width { get; }
        public InstructionOperand[] Operands { get; }

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth width, InstructionOperand[] operands)
        {
            OpCode = opCode;
            Type = type;
            Width = width;
            Length = length;
            Operands = operands;
        }

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth width, InstructionOperand dest, InstructionOperand source)
            : this(opCode, length, type, width, new[] { dest, source }) { }

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth dataType, InstructionOperand dest)
            : this(opCode, length, type, dataType, new[] { dest }) { }

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth dataType) : this(opCode, length, type, dataType, Array.Empty<InstructionOperand>()) { }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(Type);
            for (int i = 0; i < Operands.Length; i++)
            {
                InstructionOperand operand = Operands[i];
                s.Append(operand.ToString());
            }
            s.Append(' ');
            s.Append('(');
            s.Append("op: ");
            s.Append(OpCode.ToString("X2"));
            s.Append(", ");
            s.Append(Length);
            s.Append(" bytes");
            s.Append(')');
            return s.ToString();
        }
    }
}
