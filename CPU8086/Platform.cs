using System;

namespace Final.CPU8086
{
    public enum PlatformType
    {
        None = 0,
        _32Bit,
        _186,
        _286,
        _386,
        _387,
        _486,
        Pentium,
    }

    public readonly struct Platform
    {
        public PlatformType Type { get; }

        public Platform(PlatformType type)
        {
            Type = type;
        }

        public static Platform Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new Platform();
            switch (value.ToLower())
            {
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
                    return new Platform(PlatformType.Pentium);
                default:
                    throw new NotSupportedException($"The platform '{value}' is not supported!");
            }
        }

        public override string ToString() => Type.ToString();
    }
}
