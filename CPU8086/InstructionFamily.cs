using System;

namespace Final.CPU8086
{
    public class InstructionFamily : IEquatable<InstructionFamily>
    {
        public string Name { get; }
        public string Description { get; }
        public Platform Platform { get; }

        public InstructionFamily(string name, string description, Platform platform)
        {
            Name = name;
            Description = description;
            Platform = platform;
        }

        public override int GetHashCode() => Name.GetHashCode();
        public bool Equals(InstructionFamily other) => Name.Equals(other.Name);
        public override bool Equals(object obj) => obj is InstructionFamily other && Equals(other);
    }
}
