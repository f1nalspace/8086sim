namespace Final.CPU8086
{
    public enum ErrorCode
    {
        Unknown = 0,
        NotEnoughBytesInStream,
        EndOfStream,
        StreamIndexOutOfRange,
        InstructionLengthTooSmall,
        InstructionLengthTooLarge,
        InstructionParameterMissing,
        ExecutionFailed,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionMissingAdditionalData,
        ModeNotImplemented,
        ConstantFieldMismatch,
        RegFieldMismatch,
    }
}
