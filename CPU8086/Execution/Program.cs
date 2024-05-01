using System;
using System.Collections.Immutable;
using System.IO;

namespace Final.CPU8086.Execution
{
    public class Program : IProgram
    {
        public string Name { get; }
        public ImmutableArray<byte> Stream { get; }
        public RegisterState Register { get; }
        public int Length => Stream.Length;

        public Program(string name, ReadOnlySpan<byte> stream, RegisterState register = default)
        {
            Name = name;

            int haltPos = BackScanForHalt(stream);

            if (haltPos == -1)
                Stream = stream.ToImmutableArray();
            else
                Stream = stream.Slice(0, haltPos + 1).ToImmutableArray();

            Register = register;
        }

        public Program(string name, Stream stream, RegisterState register = default)
        {
            Name = name;

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            int haltPos = BackScanForHalt(data);

            if (haltPos == -1)
                Stream = data.ToImmutableArray();
            else
                Stream = data.AsSpan(0, haltPos + 1).ToImmutableArray();

            Register = register;
        }

        public static int BackScanForHalt(ReadOnlySpan<byte> data)
        {
            for (int index = data.Length - 1; index > 0; index--)
            {
                byte opCode = data[index];
                if (opCode == 0xF4 && index < data.Length - 2)
                {
                    byte nextByte = data[index + 1];
                    if (nextByte == 0)
                        return index;
                }
            }
            return -1;
        }

        public override string ToString() => Name;
    }
}
