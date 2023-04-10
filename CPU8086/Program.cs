using System.Collections.Immutable;
using System.IO;

namespace Final.CPU8086
{
    public class Program : IProgram
    {
        public string Name { get; }
        public ImmutableArray<byte> Stream { get; }
        public CPURegister Register { get; }
        public int Length => Stream.Length;

        public Program(string name, ImmutableArray<byte> stream, CPURegister register = default)
        {
            Name = name;
            Stream = stream;
            Register = register;
        }

        public Program(string name, Stream stream, CPURegister register = default)
        {
            Name = name;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            Stream = data.ToImmutableArray();
            Register = register;
        }

        public override string ToString() => Name;
    }
}
