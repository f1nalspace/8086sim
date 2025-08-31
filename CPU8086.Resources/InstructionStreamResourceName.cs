using System;
using System.IO;

namespace Final.CPU8086
{
    public class InstructionStreamResourceName
    {
        public string ResourceName { get; }
        public string Group { get; }
        public string Name { get; }
        public string Extension { get; }

        public InstructionStreamResourceName(string resourceName, string group, string name)
        {
            ResourceName = resourceName;
            Group = group;
            Name = name;
            Extension = Path.GetExtension(name);
        }

        public bool IsBinary => ("performance_aware".Equals(Group) && string.IsNullOrEmpty(Extension)) || ".bin".Equals(Extension, StringComparison.OrdinalIgnoreCase);

        public override string ToString() => ResourceName;
    }
}
