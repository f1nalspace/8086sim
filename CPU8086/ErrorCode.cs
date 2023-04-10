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
        MissingInstructionParameter,
        MissingProgramParameter,
        ExecutionFailed,
        MissingExecutionFunction,
        MismatchInstructionOperands,
        UnsupportedOperandType,
        UnsupportedRegisterType,
        UnsupportedEffectiveAddressCalculation,
        InvalidMemoryAddress,
        UnsupportedDataWidth,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionMissingAdditionalData,
        ModeNotImplemented,
        ConstantFieldMismatch,
        RegFieldMismatch,
    }
}
