using System;
using System.Text;

namespace Final.CPU8086
{
    public class Instruction : IEquatable<Instruction>
    {
        public uint Position { get; }
        public byte OpCode { get; }
        public byte Length { get; }
        public Mnemonic Mnemonic { get; }
        public DataWidth Width { get; }
        public InstructionFlags Flags { get; }
        public InstructionOperand[] Operands { get; }

        public InstructionOperand FirstOperand => Operands.Length > 0 ? Operands[0] : new InstructionOperand();
        public InstructionOperand SecondOperand => Operands.Length > 1 ? Operands[1] : new InstructionOperand();

        public Instruction(uint position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionFlags flags, InstructionOperand[] operands)
        {
            Position = position;
            OpCode = opCode;
            Mnemonic = mnemonic;
            Width = width;
            Flags = flags;
            Length = length;
            Operands = operands;
        }

        public Instruction(uint position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionFlags flags, ReadOnlySpan<InstructionOperand> operands)
        {
            Position = position;
            OpCode = opCode;
            Mnemonic = mnemonic;
            Width = width;
            Flags = flags;
            Length = length;
            Operands = operands.ToArray();
        }

        public Instruction(uint position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionFlags flags, InstructionOperand dest, InstructionOperand source)
            : this(position, opCode, length, mnemonic, width, flags, new[] { dest, source }) { }

        public Instruction(uint position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionFlags flags, InstructionOperand dest)
            : this(position, opCode, length, mnemonic, width, flags, new[] { dest }) { }

        public Instruction(uint position, byte opCode, byte length, Mnemonic mnemonic, DataWidth width, InstructionFlags flags) : this(position, opCode, length, mnemonic, width, flags, Array.Empty<InstructionOperand>()) { }

        public bool Equals(Instruction other)
        {
            if (OpCode != other.OpCode) return false;
            if (Length != other.Length) return false;
            if (!Mnemonic.Equals(other.Mnemonic)) return false;
            if (!Width.Equals(other.Width)) return false;
            if (!Flags.Equals(other.Flags)) return false;
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
        public override int GetHashCode() => HashCode.Combine(Position, OpCode, Length, Mnemonic, Width, Flags);

        public string Asm(OutputValueMode outputMode = OutputValueMode.Auto, string hexPrefix = "0x")
        {
            StringBuilder s = new StringBuilder();
            s.Append(Mnemonic.ToString());

            bool hadRegister = false;
            foreach (InstructionOperand op in Operands)
            {
                if (op.Op == OperandType.Register)
                    hadRegister |= true;
            }

            string separator = string.Empty;
            foreach (InstructionOperand op in Operands)
            {
                if (op.Op == OperandType.None)
                    continue;

                s.Append(separator);
                separator = ",";

                switch (op.Op)
                {
                    case OperandType.Register:
                        {
                            s.Append(' ');
                            s.Append(Register.GetName(op.Register));
                        }
                        break;
                    case OperandType.Address:
                        {
                            MemoryAddress mem = op.Memory;
                            if (Flags.HasFlag(InstructionFlags.Far))
                                s.Append(" FAR");

                            if (!hadRegister)
                            {
                                if (Width.Type == DataWidthType.Word)
                                    s.Append(" WORD");
                                else
                                    s.Append(" BYTE");
                            }

                            s.Append(' ');
                            s.Append(mem.Asm(Width.Type, outputMode, hexPrefix));
                        }
                        break;
                    case OperandType.Immediate:
                        {
                            Immediate imm = op.Immediate;
                            s.Append(' ');
                            s.Append(imm.Asm(Width.Type, outputMode, hexPrefix));
                        }
                        break;
                    case OperandType.Value:
                        {

                        }
                        break;
                    default:
                        break;
                }
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
            s.Append("position: ");
            s.Append(Position);
            s.Append(", ");
            s.Append("length: ");
            s.Append(Length);
            s.Append(", ");
            s.Append("width: ");
            s.Append(Width);
            s.Append(')');
            return s.ToString();
        }
    }
}
