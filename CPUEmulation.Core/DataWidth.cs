using System;
using System.Diagnostics.CodeAnalysis;

namespace Final.CPUEmulation
{
    public enum DataWidthType : byte
    {
        None = 0,
        Byte = 1,
        Word = 2,
        DoubleWord = 4,
        QuadWord = 8,
    }

    public readonly struct DataWidth : IEquatable<DataWidth>
    {
        public DataWidthType Type { get; }

        public DataWidth(DataWidthType type)
        {
            Type = type;
        }

        public static bool TryParse(string text, out DataWidth result)
        {
            DataWidthType type = (text ?? string.Empty) switch
            {
                "B" => DataWidthType.Byte,
                "W" => DataWidthType.Word,
                "D" => DataWidthType.DoubleWord,
                "Q" => DataWidthType.QuadWord,
                _ => DataWidthType.None,
            };
            result = new DataWidth(type);
            return true;
        }

        public static DataWidth Parse(string text)
        {
            if (!TryParse(text, out DataWidth result))
                throw new NotSupportedException($"The data width '{text}' is not supported!");
            return result;
        }

        public static implicit operator DataWidth(string value) => Parse(value);
        public static implicit operator DataWidth(DataWidthType type) => new DataWidth(type);
        public static explicit operator string(DataWidth dw) => dw.ToString();

        public bool Equals(DataWidth other) => Type == other.Type;
        public override bool Equals([NotNullWhen(true)] object obj) => obj is DataWidth dw && Equals(dw);
        public override int GetHashCode() => Type.GetHashCode();

        public static bool operator ==(DataWidth dw1, DataWidth dw2) => dw1.Equals(dw2);
        public static bool operator !=(DataWidth dw1, DataWidth dw2) => !dw1.Equals(dw2);

        public override string ToString()
        {
            return Type switch
            {
                DataWidthType.Byte => "B",
                DataWidthType.Word => "W",
                DataWidthType.DoubleWord => "D",
                DataWidthType.QuadWord => "Q",
                _ => string.Empty,
            };
        }

        public static readonly DataWidth None = new DataWidth(DataWidthType.None);
        public static readonly DataWidth Byte = new DataWidth(DataWidthType.Byte);
        public static readonly DataWidth Word = new DataWidth(DataWidthType.Word);
        public static readonly DataWidth DoubleWord = new DataWidth(DataWidthType.DoubleWord);
        public static readonly DataWidth QuadWord = new DataWidth(DataWidthType.QuadWord);
    }
}
