using System;

namespace Final.CPUEmulation
{
    public class Platform : IEquatable<Platform>
    {
        public uint Id { get; }
        public string Name { get; }

        public Platform(uint id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => $"{Name} [{Id}]";

        public bool Equals(Platform other) => other != null && Id.Equals(other.Id);
        public override bool Equals(object obj) => obj is Platform platform && Equals(platform);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
