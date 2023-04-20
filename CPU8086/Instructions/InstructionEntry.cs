using Final.CPU8086.Types;
using OneOf.Types;
using System.Text;

namespace Final.CPU8086.Instructions
{
    public class InstructionEntry
    {
        public byte Op { get; }
        public Mnemonic Mnemonic { get; }
        public DataWidth DataWidth { get; }
        public InstructionFlags Flags { get; set; }
        public DataType DataType { get; }
        public Flags UsedFlags { get; }
        public Platform Platform { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public Field[] Fields { get; }
        public Operand[] Operands { get; }
        public string Description { get; set; }

        public InstructionType Type => Mnemonic.Type;

        public InstructionEntry(byte op, Mnemonic mnemonic, DataWidth dataWidth, InstructionFlags flags, DataType dataType, Flags usedFlags, Platform platform, int minLength, int maxLength, Field[] fields, Operand[] operands)
        {
            Op = op;
            DataWidth = dataWidth;
            Flags = flags;
            DataType = dataType;
            UsedFlags = usedFlags;
            Mnemonic = mnemonic;
            Platform = platform;
            MinLength = minLength;
            MaxLength = maxLength;
            Fields = fields;
            Operands = operands;
        }

        static readonly InstructionFlags[] PossibleFlags = new[] {
            InstructionFlags.Lock,
            InstructionFlags.Rep,
            InstructionFlags.Segment,
            InstructionFlags.Far,
            InstructionFlags.SignExtendedImm8,
            InstructionFlags.Prefix,
            InstructionFlags.Override,
        };

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
                s.Append('[');
                int c = 0;
                foreach (var flag in PossibleFlags)
                {
                    if (Flags.HasFlag(flag))
                    {
                        if (c > 0)
                            s.Append(", ");
                        s.Append(flag.ToString());
                        ++c;
                    }
                }
                s.Append(']');
            }
            s.Append('|');
            s.Append(DataType);
            s.Append('|');
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
            s.Append(UsedFlags.ToString());
            s.Append('|');
            s.Append(Platform);
            return s.ToString();
        }
    }
}
