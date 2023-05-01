using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Final.CPU8086.Instructions
{
    public enum FieldDefinitionType : byte
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

    public readonly struct FieldDefinition
    {
        private static readonly Regex _rexConstantExpression = new Regex("(?<hex>[0-9a-fA-F]{2})(?<exp>\\+[a-z])?", RegexOptions.Compiled);

        public FieldDefinitionType Type { get; }
        public byte Value { get; }
        public FieldExpression Expression { get; }

        public FieldDefinition(FieldDefinitionType type, byte value, FieldExpression expression = FieldExpression.None)
        {
            Type = type;
            Value = value;
            Expression = expression;
        }

        public static FieldDefinition Parse(string value)
        {
            FieldDefinitionType type = value switch
            {
                "mr" => FieldDefinitionType.ModRegRM,
                "d0" => FieldDefinitionType.Displacement0,
                "d1" => FieldDefinitionType.Displacement1,
                "i0" => FieldDefinitionType.Immediate0,
                "i1" => FieldDefinitionType.Immediate1,
                "i2" => FieldDefinitionType.Immediate2,
                "i3" => FieldDefinitionType.Immediate3,
                "o0" => FieldDefinitionType.Offset0,
                "o1" => FieldDefinitionType.Offset1,
                "s0" => FieldDefinitionType.Segment0,
                "s1" => FieldDefinitionType.Segment1,
                "r0" => FieldDefinitionType.RelativeLabelDisplacement0,
                "r1" => FieldDefinitionType.RelativeLabelDisplacement1,
                "/0" => FieldDefinitionType.Mod000RM,
                "/1" => FieldDefinitionType.Mod001RM,
                "/2" => FieldDefinitionType.Mod010RM,
                "/3" => FieldDefinitionType.Mod011RM,
                "/4" => FieldDefinitionType.Mod100RM,
                "/5" => FieldDefinitionType.Mod101RM,
                "/6" => FieldDefinitionType.Mod110RM,
                "/7" => FieldDefinitionType.Mod111RM,
                "sl" => FieldDefinitionType.ShortLabelOrShortLow,
                "ll" => FieldDefinitionType.LongLabel,
                "sh" => FieldDefinitionType.ShortHigh,
                "i0~i3" => FieldDefinitionType.Immediate0to3,
                _ => FieldDefinitionType.None,
            };
            if (type == FieldDefinitionType.None)
            {
                Match m = _rexConstantExpression.Match(value);
                if (m.Success)
                {
                    byte constantValue = byte.Parse(m.Groups["hex"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    string expressionRaw = m.Groups["exp"].Value?.ToLower();
                    if (string.IsNullOrWhiteSpace(expressionRaw))
                        return new FieldDefinition(FieldDefinitionType.Constant, constantValue);
                    else
                    {
                        FieldExpression expression = expressionRaw switch
                        {
                            "+i" => FieldExpression.Plus_I,
                            _ => throw new NotSupportedException($"The expression '{expressionRaw}' is not supported!")
                        };
                        return new FieldDefinition(FieldDefinitionType.Constant, constantValue, expression);
                    }
                }
                else
                    throw new NotSupportedException($"The value '{value}' is not supported!");
            }
            return new FieldDefinition(type, 0);
        }

        public static implicit operator FieldDefinition(string value) => Parse(value);
        public static explicit operator string(FieldDefinition field) => field.ToString();

        public override string ToString()
        {
            switch (Type)
            {
                case FieldDefinitionType.Constant:
                    {
                        if (Expression != FieldExpression.None)
                            return $"{Value:X2}{Expression}";
                        else
                            return $"{Value:X2}";
                    }
                case FieldDefinitionType.ModRegRM:
                    return "mr";
                case FieldDefinitionType.Mod000RM:
                    return "/0";
                case FieldDefinitionType.Mod001RM:
                    return "/1";
                case FieldDefinitionType.Mod010RM:
                    return "/2";
                case FieldDefinitionType.Mod011RM:
                    return "/3";
                case FieldDefinitionType.Mod100RM:
                    return "/4";
                case FieldDefinitionType.Mod101RM:
                    return "/5";
                case FieldDefinitionType.Mod110RM:
                    return "/6";
                case FieldDefinitionType.Mod111RM:
                    return "/7";
                case FieldDefinitionType.Displacement0:
                    return "d0";
                case FieldDefinitionType.Displacement1:
                    return "d1";
                case FieldDefinitionType.Immediate0:
                    return "i0";
                case FieldDefinitionType.Immediate1:
                    return "i1";
                case FieldDefinitionType.Immediate2:
                    return "i2";
                case FieldDefinitionType.Immediate3:
                    return "i3";
                case FieldDefinitionType.Immediate0to3:
                    return "i0~i3";
                case FieldDefinitionType.Offset0:
                    return "o0";
                case FieldDefinitionType.Offset1:
                    return "o1";
                case FieldDefinitionType.Segment0:
                    return "s0";
                case FieldDefinitionType.Segment1:
                    return "s1";
                case FieldDefinitionType.RelativeLabelDisplacement0:
                    return "r0";
                case FieldDefinitionType.RelativeLabelDisplacement1:
                    return "r1";
                case FieldDefinitionType.ShortLabelOrShortLow:
                    return "sl";
                case FieldDefinitionType.LongLabel:
                    return "ll";
                case FieldDefinitionType.ShortHigh:
                    return "sh";
                default:
                    return string.Empty;
            }
        }
    }
}
