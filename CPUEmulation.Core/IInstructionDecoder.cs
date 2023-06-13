using OneOf;
using System;

namespace Final.CPUEmulation
{
    public interface IInstructionDecoder
    {
        OneOf<Instruction, Error> DecodeNext(ReadOnlySpan<byte> stream, string streamName, uint position);
    }
}
