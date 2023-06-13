using System;

namespace Final.CPUEmulation
{
    public class Mnemonic
    {
        public uint Id { get; }
        public string Name { get; }

        public Mnemonic(uint id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Id = id;
            Name = name;
        }

        public override string ToString() => $"{Name} [{Id}]";
    }
}
