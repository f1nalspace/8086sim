using System.Text;

namespace Final.CPU8086
{
    public class InstructionEntry
    {
        public byte Op { get; }

        public InstructionFamily Family { get; }
        public Platform Platform { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public Operand[] Operands { get; }
        public Field[] Fields { get; }

        public InstructionEntry(byte op, InstructionFamily family, Platform platform, int minLength, int maxLength, Operand[] operands, Field[] fields)
        {
            Op = op;
            Family = family;
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
            s.Append(Family.Name);
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
