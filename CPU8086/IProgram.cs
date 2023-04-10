using System.Collections.Immutable;

namespace Final.CPU8086
{
    public interface IProgram
    {
        string Name { get; }
        ImmutableArray<byte> Stream { get; }
        CPURegister Register { get; }
    }
}
