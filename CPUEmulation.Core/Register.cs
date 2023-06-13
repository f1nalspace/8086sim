using System;

namespace Final.CPUEmulation
{
    public readonly struct Register : IEquatable<Register>
    {
        public byte Id { get; }
        public DataWidth Width { get; }

        public Register(byte id, DataWidth width)
        {
            Id = id;
            Width = width;
        }

        public bool Equals(Register other) => Id.Equals(other.Id) && Width.Equals(other.Width);
        public override bool Equals(object obj) => obj is Register reg && Equals(reg);
        public static bool operator ==(Register left, Register right) => left.Equals(right);
        public static bool operator !=(Register left, Register right) => !left.Equals(right);
        public override int GetHashCode() => HashCode.Combine(Id, Width);

        public override string ToString() => $"{Id} [{Width}]";
    }
}
