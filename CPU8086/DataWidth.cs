using System;
using System.Diagnostics.CodeAnalysis;

namespace Final.CPU8086
{
    public enum DataWidthType : byte
    {
        None = 0,
        Byte = 1,
        Word = 2,
        DoubleWord = 4,
        QuadWord = 8,
        TenBytes = 10,
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
                "T" => DataWidthType.TenBytes,
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

        public override string ToString()
        {
            return Type switch
            {
                DataWidthType.Byte => "B",
                DataWidthType.Word => "W",
                DataWidthType.DoubleWord => "D",
                DataWidthType.QuadWord => "Q",
                DataWidthType.TenBytes => "T",
                _ => string.Empty,
            };
        }

        
    }
}
