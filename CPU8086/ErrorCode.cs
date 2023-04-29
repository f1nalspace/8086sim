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
        ProgramNotLoaded,

        // Execution errors
        ExecutionFailed,
        MissingExecutionFunction,
        MismatchInstructionOperands,

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

        // Execute errors
        InvalidExecutionState,
        InvalidSegmentAddress,
        EndOfProgram,
        MissingStateParameter,
        UnsupportedInstruction,
    }
}
