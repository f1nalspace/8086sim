using System;
using System.Text;

namespace Final.CPU8086
{
    public readonly struct Instruction : IEquatable<Instruction>
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

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth width, ReadOnlySpan<InstructionOperand> operands)
        {
            OpCode = opCode;
            Type = type;
            Width = width;
            Length = length;
            Operands = operands.ToArray();
        }

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth width, InstructionOperand dest, InstructionOperand source)
            : this(opCode, length, type, width, new[] { dest, source }) { }

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth width, InstructionOperand dest)
            : this(opCode, length, type, width, new[] { dest }) { }

        public Instruction(byte opCode, byte length, InstructionType type, DataWidth width) : this(opCode, length, type, width, Array.Empty<InstructionOperand>()) { }

        public bool Equals(Instruction other)
        {
            if (OpCode!= other.OpCode) return false;
            if (Length != other.Length) return false;
            if (Type != other.Type) return false;
            if (!Width.Equals(other.Width)) return false;
            if (Operands.Length != other.Operands.Length) return false;
            for (int i = 0; i < Operands.Length; ++i)
            {
                if (!Operands[i].Equals(other.Operands[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj) => obj is Instruction instruction && Equals(instruction);
        public override int GetHashCode() => Type.GetHashCode();

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
