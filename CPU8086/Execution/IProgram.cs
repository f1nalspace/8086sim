using System.Collections.Immutable;

namespace Final.CPU8086.Execution
{
    public interface IProgram
    {
        string Name { get; }
        ImmutableArray<byte> Stream { get; }
        RegisterState Register { get; }
        int Length { get; }
    }
}
