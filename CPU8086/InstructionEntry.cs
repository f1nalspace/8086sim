using System.Text;

namespace Final.CPU8086
{
    public class InstructionEntry
    {
        public byte Op { get; }
        public InstructionType Type { get; }
        public DataWidth DataWidth { get; }
        public DataFlags DataFlags { get; }
        public InstructionFlags Flags { get; }
        public Platform Platform { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public Operand[] Operands { get; }
        public Field[] Fields { get; }

        public InstructionEntry(byte op, InstructionType type, DataWidth dataWidth, DataFlags dataFlags, InstructionFlags flags, Platform platform, int minLength, int maxLength, Operand[] operands, Field[] fields)
        {
            Op = op;
            DataWidth = dataWidth;
            DataFlags = dataFlags;
            Flags = flags;
            Type = type;
            Platform = platform;
            MinLength = minLength;
            MaxLength = maxLength;
            Operands = operands;
            Fields = fields;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("0x");
            s.Append(Op.ToString("X2"));
            s.Append('|');
            s.Append(Type);
            s.Append('|');
            s.Append(DataWidth);
            if (DataFlags != DataFlags.None)
            {
                s.Append(' ');
                s.Append('[');
                int c = 0;
                if (DataFlags.HasFlag(DataFlags.SignExtendedImm8))
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
                s.Append("|");
                for (int i = 0; i < Fields.Length; i++)
                {
                    if (i > 0)
                        s.Append(", ");
                    s.Append(Fields[i].ToString());
                }
            }
            s.Append('|');
            s.Append(Flags.ToString());
            if (Platform.Type != PlatformType.None)
            {
                s.Append(' ');
                s.Append('[');
                s.Append(Platform);
                s.Append(']');
            }
            return s.ToString();
        }
    }
}
