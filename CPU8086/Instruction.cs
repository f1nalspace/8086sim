using System;
using System.Text;

namespace Final.CPU8086
{
    public class Instruction : IEquatable<Instruction>
    {
        public int Position { get; }
        public byte OpCode { get; }
        public byte Length { get; }
        public Mnemonic Mnemonic { get; }
        public DataWidth Width { get; }
        public InstructionOperand[] Operands { get; }

        public Instruction(int position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionOperand[] operands)
        {
            Position = position;
            OpCode = opCode;
            Mnemonic = mnemonic;
            Width = width;
            Length = length;
            Operands = operands;
        }

        public Instruction(int position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, ReadOnlySpan<InstructionOperand> operands)
        {
            Position = position;
            OpCode = opCode;
            Mnemonic = mnemonic;
            Width = width;
            Length = length;
            Operands = operands.ToArray();
        }

        public Instruction(int position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionOperand dest, InstructionOperand source)
            : this(position, opCode, length, mnemonic, width, new[] { dest, source }) { }

        public Instruction(int position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionOperand dest)
            : this(position, opCode, length, mnemonic, width, new[] { dest }) { }

        public Instruction(int position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width) : this(position, opCode, length, mnemonic, width, Array.Empty<InstructionOperand>()) { }

        public bool Equals(Instruction other)
        {
            if (OpCode != other.OpCode) return false;
            if (Length != other.Length) return false;
            if (!Mnemonic.Equals(other.Mnemonic)) return false;
            if (!Width.Equals(other.Width)) return false;
            if (Position != other.Position) return false;
            if (Operands.Length != other.Operands.Length) return false;
            for (int i = 0; i < Operands.Length; ++i)
            {
                if (!Operands[i].Equals(other.Operands[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj) => obj is Instruction instruction && Equals(instruction);
        public override int GetHashCode() => HashCode.Combine(OpCode, Length,  Mnemonic, Width, Position);

        public string Asm(OutputValueMode outputMode = OutputValueMode.Auto, string hexPrefix = "0x")
        {
            StringBuilder s = new StringBuilder();
            s.Append(Mnemonic.ToString());
            int count = 0;
            foreach (InstructionOperand op in Operands)
            {
                if (count == 1)
                    s.Append(',');
                s.Append(' ');
                s.Append(op.Asm(Width.Type, outputMode, hexPrefix));
                ++count;
            }
            return s.ToString();
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(Mnemonic);
            if (Operands != null && Operands.Length > 0)
            {
                s.Append(' ');
                for (int i = 0; i < Operands.Length; i++)
                {
                    if (i > 0)
                        s.Append(", ");
                    InstructionOperand operand = Operands[i];
                    s.Append(operand.ToString());
                }
            }
            s.Append(' ');
            s.Append('(');
            s.Append("op: ");
            s.Append(OpCode.ToString("X2"));
            s.Append(", ");
            s.Append("length: ");
            s.Append(Length);
            s.Append(", ");
            s.Append("index: ");
            s.Append(Position);
            s.Append(')');
            return s.ToString();
        }
    }
}
