using System.Text;

namespace Final.CPU8086
{
    public class InstructionEntry
    {
        public byte Op { get; }
        public Mnemonic Mnemonic { get; }
        public DataWidth DataWidth { get; }
        public InstructionFlags Flags { get; set; }
        public States States { get; }
        public Platform Platform { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public Field[] Fields { get; }
        public Operand[] Operands { get; }
        public string Description { get; set; }

        public InstructionType Type => Mnemonic.Type;

        public InstructionEntry(byte op, Mnemonic mnemonic, DataWidth dataWidth, InstructionFlags flags, States states, Platform platform, int minLength, int maxLength, Field[] fields, Operand[] operands)
        {
            Op = op;
            DataWidth = dataWidth;
            Flags = flags;
            States = states;
            Mnemonic = mnemonic;
            Platform = platform;
            MinLength = minLength;
            MaxLength = maxLength;
            Fields = fields;
            Operands = operands;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("0x");
            s.Append(Op.ToString("X2"));
            s.Append('|');
            s.Append(Mnemonic);
            s.Append('|');
            s.Append(DataWidth);
            if (Flags != InstructionFlags.None)
            {
                s.Append(' ');
                s.Append('[');
                int c = 0;
                if (Flags.HasFlag(InstructionFlags.SignExtendedImm8))
                {
                    if (c > 0)
                        s.Append(", ");
                    s.Append("SignExtended");
                    ++c;
                }
                s.Append(']');
            }
            foreach (Operand operand in Operands)
            {
                s.Append(' ');
                s.Append(operand);
            }
            s.Append('|');
            s.Append(MinLength);
            s.Append('|');
            s.Append(MaxLength);
            if (Fields.Length > 0)
            {
                s.Append('|');
                for (int i = 0; i < Fields.Length; i++)
                {
                    if (i > 0)
                        s.Append(", ");
                    s.Append(Fields[i].ToString());
                }
            }
            s.Append('|');
            s.Append(States.ToString());
            s.Append('|');
            s.Append(Platform);
            return s.ToString();
        }
    }
}
