using System.Collections.Immutable;

namespace Final.CPU8086
{
    public interface IProgram
    {
        string Name { get; }
        ImmutableArray<byte> Stream { get; }
        RegisterState Register { get; }
        int Length { get; }
    }
}
