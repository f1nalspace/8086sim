using System;

namespace Final.CPU8086.Types
{
    public enum PlatformType
    {
        _8086 = 0,
        _8087,
        _8088,
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

        public static implicit operator Platform(string value) => Parse(value);
        public static explicit operator string(Platform platform) => platform.ToString();

        public static Platform Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Platform8086;
            return value.ToLower() switch
            {
                "8087" => new Platform(PlatformType._8087),
                "8088" => new Platform(PlatformType._8088),
                "186" => new Platform(PlatformType._186),
                "286" => new Platform(PlatformType._286),
                "386" => new Platform(PlatformType._386),
                "387" => new Platform(PlatformType._387),
                "486" => new Platform(PlatformType._486),
                "32bit" => new Platform(PlatformType._32Bit),
                "p5" => new Platform(PlatformType._586),
                _ => Platform8086,
            };
        }

        public override string ToString()
        {
            return Type switch
            {
                PlatformType._8086 => "8086",
                PlatformType._8087 => "8087",
                PlatformType._8088 => "8088",
                PlatformType._186 => "186",
                PlatformType._286 => "286",
                PlatformType._386 => "386",
                PlatformType._387 => "387",
                PlatformType._486 => "486",
                PlatformType._32Bit => "32bit",
                PlatformType._586 => "p5",
                _ => string.Empty,
            };
        }

        public static readonly Platform Platform8086 = new Platform(PlatformType._8086);
        public static readonly Platform Platform8088 = new Platform(PlatformType._8088);
    }
}
