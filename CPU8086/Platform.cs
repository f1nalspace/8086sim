using System;

namespace Final.CPU8086
{
    public enum PlatformType
    {
        None = 0,
        _8087,
        _186,
        _286,
        _386,
        _387,
        _32Bit,
        _486,
        _586,
    }

    public readonly struct Platform : IEquatable<Platform>
    {
        public PlatformType Type { get; }

        public Platform(PlatformType type)
        {
            Type = type;
        }

        public bool Equals(Platform other) => Type == other.Type;
        public override int GetHashCode() => Type.GetHashCode();
        public override bool Equals(object obj) => obj is Platform platform && Equals(platform);

        public static bool operator ==(Platform left, Platform right) => left.Equals(right);
        public static bool operator !=(Platform left, Platform right) => !(left == right);
        public static bool operator <(Platform left, Platform right) => left.Type < right.Type;
        public static bool operator >(Platform left, Platform right) => left.Type > right.Type;

        public static Platform Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new Platform();
            switch (value.ToLower())
            {
                case "8087":
                    return new Platform(PlatformType._8087);
                case "186":
                    return new Platform(PlatformType._186);
                case "286":
                    return new Platform(PlatformType._286);
                case "386":
                    return new Platform(PlatformType._386);
                case "387":
                    return new Platform(PlatformType._387);
                case "486":
                    return new Platform(PlatformType._486);
                case "32bit":
                    return new Platform(PlatformType._32Bit);
                case "p5":
                    return new Platform(PlatformType._586);
                default:
                    throw new NotSupportedException($"The platform '{value}' is not supported!");
            }
        }

        public override string ToString() => Type.ToString();

        
    }
}
