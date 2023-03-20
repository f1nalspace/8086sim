namespace Final.CPU8086
{
    public enum ErrorCode
    {
        Unknown = 0,
        NotEnoughBytesInStream,
        EndOfStream,
        TooSmallAdvancementInStream,
        TooLargeAdvancementInStream,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionMissingAdditionalData,
        ModeNotImplemented,
    }
}
