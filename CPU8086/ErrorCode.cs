namespace Final.CPU8086
{
    public enum ErrorCode
    {
        Unknown = 0,
        NotEnoughBytesInStream,
        TooSmallAdvancementInStream,
        TooLargeAdvancementInStream,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionMissingAdditionalData,
        ModeNotImplemented,
    }
}
