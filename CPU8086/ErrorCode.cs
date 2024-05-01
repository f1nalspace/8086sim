namespace Final.CPU8086
{
    public enum ErrorCode
    {
        Unknown = 0,

        // Stream errors
        NotEnoughBytesInStream,
        EndOfStream,
        StreamIndexOutOfRange,

        // Instruction errors
        InstructionLengthTooSmall,
        InstructionLengthTooLarge,
        MissingInstructionParameter,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionMissingAdditionalData,
        ModeNotImplemented,

        // Program errors
        MissingProgramParameter,
        ProgramTooLarge,
        ProgramIsEmpty,
        ProgramNotLoaded,

        // Execution errors
        ExecutionStopped,
        ExecutionFailed,
        MissingExecutionFunction,
        MismatchInstructionOperands,
        InvalidExecutionState,
        InvalidSegmentAddress,
        EndOfProgram,
        MissingStateParameter,
        UnsupportedInstruction,

        // Type errors
        UnsupportedOperandType,
        UnsupportedRegisterType,
        UnsupportedEffectiveAddressCalculation,
        UnsupportedImmediateFlags,
        UnsupportedDataWidth,
        UnsupportedDataType,

        // Memory errors
        InvalidMemoryAddress,

        // Fields errors
        ConstantFieldMismatch,
        RegFieldMismatch,
        JumpInstructionNotFound,
        InvalidOperandsLength,
        UnsupportedFieldType,

        // Load/Store errors
        FailedToLoadRegister,
        FailedToLoadMemory,
    }
}
