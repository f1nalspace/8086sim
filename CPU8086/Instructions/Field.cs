using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Final.CPU8086.Instructions
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
        public byte Value { get; }
        public FieldExpression Expression { get; }

        public Field(FieldType type, byte value, FieldExpression expression = FieldExpression.None)
        {
            Type = type;
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
                        return new Field(FieldType.Constant, constantValue);
                    else
                    {
                        FieldExpression expression = expressionRaw switch
                        {
                            "+i" => FieldExpression.Plus_I,
                            _ => throw new NotSupportedException($"The expression '{expressionRaw}' is not supported!")
                        };
                        return new Field(FieldType.Constant, constantValue, expression);
                    }
                }
                else
                    throw new NotSupportedException($"The value '{value}' is not supported!");
            }
            return new Field(type, 0);
        }

        public static implicit operator Field(string value) => Parse(value);
        public static explicit operator string(Field field) => field.ToString();

        public override string ToString()
        {
            switch (Type)
            {
                case FieldType.Constant:
                    {
                        if (Expression != FieldExpression.None)
                            return $"{Value:X2}{Expression}";
                        else
                            return $"{Value:X2}";
                    }
                case FieldType.ModRegRM:
                    return "mr";
                case FieldType.Mod000RM:
                    return "/0";
                case FieldType.Mod001RM:
                    return "/1";
                case FieldType.Mod010RM:
                    return "/2";
                case FieldType.Mod011RM:
                    return "/3";
                case FieldType.Mod100RM:
                    return "/4";
                case FieldType.Mod101RM:
                    return "/5";
                case FieldType.Mod110RM:
                    return "/6";
                case FieldType.Mod111RM:
                    return "/7";
                case FieldType.Displacement0:
                    return "d0";
                case FieldType.Displacement1:
                    return "d1";
                case FieldType.Immediate0:
                    return "i0";
                case FieldType.Immediate1:
                    return "i1";
                case FieldType.Immediate2:
                    return "i2";
                case FieldType.Immediate3:
                    return "i3";
                case FieldType.Immediate0to3:
                    return "i0~i3";
                case FieldType.Offset0:
                    return "o0";
                case FieldType.Offset1:
                    return "o1";
                case FieldType.Segment0:
                    return "s0";
                case FieldType.Segment1:
                    return "s1";
                case FieldType.RelativeLabelDisplacement0:
                    return "r0";
                case FieldType.RelativeLabelDisplacement1:
                    return "r1";
                case FieldType.ShortLabelOrShortLow:
                    return "sl";
                case FieldType.LongLabel:
                    return "ll";
                case FieldType.ShortHigh:
                    return "sh";
                default:
                    return string.Empty;
            }
        }
    }
}
