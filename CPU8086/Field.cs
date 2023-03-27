using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Final.CPU8086
{
    public enum FieldType : byte
    {
        None = 0,
        Constant,
        ModRegRM,
        Mod000RM,
        Mod001RM,
        Mod010RM,
        Mod011RM,
        Mod100RM,
        Mod101RM,
        Mod110RM,
        Mod111RM,
        Displacement0,
        Displacement1,
        Immediate0,
        Immediate1,
        Immediate2,
        Immediate3,
        Immediate0to3,
        Offset0,
        Offset1,
        Segment0,
        Segment1,
        RelativeLabelDisplacement0,
        RelativeLabelDisplacement1,
        ShortLabelOrShortLow,
        LongLabel,
        ShortHigh,
    }

    public enum FieldExpression
    {
        None = 0,
        Plus_I
    }

    public readonly struct Field
    {
        private static readonly Regex _rexConstantExpression = new Regex("(?<hex>[0-9a-fA-F]{2})(?<exp>\\+[a-z])?", RegexOptions.Compiled);

        public FieldType Type { get; }
        public string Raw { get; }
        public byte Value { get; }
        public FieldExpression Expression { get; }

        public Field(FieldType type, string raw, byte value, FieldExpression expression = FieldExpression.None)
        {
            Type = type;
            Raw = raw;
            Value = value;
            Expression = expression;
        }

        public static Field Parse(string value)
        {
            FieldType type = value switch
            {
                "mr" => FieldType.ModRegRM,
                "d0" => FieldType.Displacement0,
                "d1" => FieldType.Displacement1,
                "i0" => FieldType.Immediate0,
                "i1" => FieldType.Immediate1,
                "i2" => FieldType.Immediate2,
                "i3" => FieldType.Immediate3,
                "o0" => FieldType.Offset0,
                "o1" => FieldType.Offset1,
                "s0" => FieldType.Segment0,
                "s1" => FieldType.Segment1,
                "r0" => FieldType.RelativeLabelDisplacement0,
                "r1" => FieldType.RelativeLabelDisplacement1,
                "/0" => FieldType.Mod000RM,
                "/1" => FieldType.Mod001RM,
                "/2" => FieldType.Mod010RM,
                "/3" => FieldType.Mod011RM,
                "/4" => FieldType.Mod100RM,
                "/5" => FieldType.Mod101RM,
                "/6" => FieldType.Mod110RM,
                "/7" => FieldType.Mod111RM,
                "sl" => FieldType.ShortLabelOrShortLow,
                "ll" => FieldType.LongLabel,
                "sh" => FieldType.ShortHigh,
                "i0~i3" => FieldType.Immediate0to3,
                _ => FieldType.None,
            };
            if (type == FieldType.None)
            {
                Match m = _rexConstantExpression.Match(value);
                if (m.Success)
                {
                    byte constantValue = byte.Parse(m.Groups["hex"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    string expressionRaw = m.Groups["exp"].Value?.ToLower();
                    if (string.IsNullOrWhiteSpace(expressionRaw))
                        return new Field(FieldType.Constant, value, constantValue);
                    else
                    {
                        FieldExpression expression = expressionRaw switch
                        {
                            "+i" => FieldExpression.Plus_I,
                            _ => throw new NotSupportedException($"The expression '{expressionRaw}' is not supported!")
                        };
                        return new Field(FieldType.Constant, value, constantValue, expression);
                    }
                }
                else
                    throw new NotSupportedException($"The value '{value}' is not supported!");
            }
            return new Field(type, value, 0);
        }

        public override string ToString()
        {
            if (Type == FieldType.Constant)
                return Value.ToString("X2");
            else if (Type != FieldType.None)
                return Type.ToString();
            else
                return Raw;
        }
    }
}
