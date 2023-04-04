using System;
using System.Text;

namespace Final.CPU8086
{
    public readonly struct Instruction : IEquatable<Instruction>
    {
        public byte OpCode { get; }
        public byte Length { get; }
        public Mnemonic Mnemonic { get; }
        public DataWidth Width { get; }
        public InstructionOperand[] Operands { get; }

        public Instruction(byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionOperand[] operands)
        {
            OpCode = opCode;
            Mnemonic = mnemonic;
            Width = width;
            Length = length;
            Operands = operands;
        }

        public Instruction(byte opCode, byte length, Mnemonic mnemonic, DataWidth width, ReadOnlySpan<InstructionOperand> operands)
        {
            OpCode = opCode;
            Mnemonic = mnemonic;
            Width = width;
            Length = length;
            Operands = operands.ToArray();
        }

        public Instruction(byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionOperand dest, InstructionOperand source)
            : this(opCode, length, mnemonic, width, new[] { dest, source }) { }

        public Instruction(byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionOperand dest)
            : this(opCode, length, mnemonic, width, new[] { dest }) { }

        public Instruction(byte opCode, byte length, Mnemonic mnemonic, DataWidth width) : this(opCode, length, mnemonic, width, Array.Empty<InstructionOperand>()) { }

        public bool Equals(Instruction other)
        {
            if (OpCode != other.OpCode) return false;
            if (Length != other.Length) return false;
            if (!Mnemonic.Equals(other.Mnemonic)) return false;
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
        public override int GetHashCode() => Mnemonic.GetHashCode();

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
            s.Append(Length);
            s.Append(" bytes");
            s.Append(", ");
            s.Append(Width);
            s.Append(')');
            return s.ToString();
        }
    }
}
