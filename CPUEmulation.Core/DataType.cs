using System;

namespace Final.CPUEmulation
{
    [Flags]
    public enum DataTypeFlags : byte
    {
        None = 0,
        Pointer = 1 << 0,
        Signed = 1 << 1,
    }

    public readonly struct DataType : IEquatable<DataType>
    {
        public DataWidth Width { get; }
        public DataTypeFlags Flags { get; }

        public DataType(DataWidth width, DataTypeFlags flags = DataTypeFlags.None)
        {
            Width = width;
            Flags = flags;
        }

        public override string ToString() => $"{Width} [{Flags}]";

        public override int GetHashCode() => HashCode.Combine(Width, Flags);
        public bool Equals(DataType other) => Width.Equals(other.Width) && Flags.Equals(other.Flags) ;
        public override bool Equals(object obj) => obj is DataType type && Equals(type);
        public static bool operator ==(DataType left, DataType right) => left.Equals(right);
        public static bool operator !=(DataType left, DataType right) => !(left == right);

        public static readonly DataType U8 = new DataType(DataWidth.Byte, DataTypeFlags.None);
        public static readonly DataType S8 = new DataType(DataWidth.Byte, DataTypeFlags.Signed);
        public static readonly DataType U16 = new DataType(DataWidth.Word, DataTypeFlags.None);
        public static readonly DataType S16 = new DataType(DataWidth.Word, DataTypeFlags.Signed);
        public static readonly DataType U32 = new DataType(DataWidth.DoubleWord, DataTypeFlags.None);
        public static readonly DataType S32 = new DataType(DataWidth.DoubleWord, DataTypeFlags.Signed);
        public static readonly DataType U64 = new DataType(DataWidth.QuadWord, DataTypeFlags.None);
        public static readonly DataType S64 = new DataType(DataWidth.QuadWord, DataTypeFlags.Signed);
    }
}
