namespace Final.CPU8086
{
    public enum ErrorCode
    {
        Unknown = 0,
        NotEnoughBytesInStream,
        EndOfStream,
        InstructionLengthTooSmall,
        InstructionLengthTooLarge,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionMissingAdditionalData,
        ModeNotImplemented,
        ConstantFieldMismatch,
        RegFieldMismatch,
    }
}
