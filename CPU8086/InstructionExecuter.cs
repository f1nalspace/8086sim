using OneOf;
using System;
using System.Diagnostics.Contracts;

namespace Final.CPU8086
{
    public delegate OneOf<int, Error> ExecuteInstructionFunction(CPU cpu, Instruction instruction);

    class InstructionExecuter
    {
        private readonly ExecuteInstructionFunction[] _typeFunctionTable;
        private readonly CPU _cpu;

        public InstructionExecuter(CPU cpu)
        {
            _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));

            Array typeValues = Enum.GetValues(typeof(InstructionType));
            _typeFunctionTable = new ExecuteInstructionFunction[typeValues.Length];
            Array.Clear(_typeFunctionTable, 0, _typeFunctionTable.Length);

            _typeFunctionTable[(int)InstructionType.MOV] = Mov;
        }

        public OneOf<int, Error> Execute(Instruction instruction)
        {
            if (instruction == null)
                return new Error(ErrorCode.MissingInstructionParameter, $"The instruction parameter is missing!", 0);
            InstructionType type = instruction.Mnemonic.Type;
            Contract.Assert((int)type < _typeFunctionTable.Length);
            ExecuteInstructionFunction func = _typeFunctionTable[(int)type];
            if (func == null)
                return new Error(ErrorCode.MissingExecutionFunction, $"No execution function for instruction type '{type}' found!", instruction.Position);
            return func(_cpu, instruction);
        }

        private static DataType WidthToType(DataWidth width)
        {
            return width.Type switch
            {
                DataWidthType.Byte => DataType.Byte,
                DataWidthType.Word => DataType.Word,
                DataWidthType.DoubleWord => DataType.DoubleWord,
                _ => DataType.None,
            };
        }

        private static OneOf<int, Error> Mov(CPU cpu, Instruction instruction)
        {
            Contract.Assert(instruction != null);

            if (instruction.Operands.Length != 2)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Invalid number of operands for {instruction.Mnemonic} instruction", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            Immediate source;
            switch (sourceOperand.Op)
            {
                case OperandType.Register:
                    {
                        OneOf<Immediate, Error> ilr = cpu.LoadRegister(sourceOperand.Register);
                        if (ilr.IsT1)
                            return new Error(ilr.AsT1, $"Failed to load source register '{sourceOperand.Register}' by instruction '{instruction}'", instruction.Position);
                        source = ilr.AsT0;
                    }
                    break;
                case OperandType.Address:
                    {
                        DataType dataType = WidthToType(instruction.Width);
                        OneOf<Immediate, Error> alr = cpu.LoadMemory(sourceOperand.Memory, dataType);
                        if (alr.IsT1)
                            return new Error(alr.AsT1, $"Failed to load source memory '{sourceOperand.Memory}' with type '{dataType}' by instruction '{instruction}'", instruction.Position);
                        source = alr.AsT0;
                    }
                    break;
                case OperandType.Immediate:
                case OperandType.Value:
                    source = sourceOperand.Immediate;
                    break;
                default:
                    return new Error(ErrorCode.UnsupportedOperandType, $"The source operand type '{sourceOperand.Op}' is not supported by instruction '{instruction}'", instruction.Position);
            }

            switch (destOperand.Op)
            {
                case OperandType.Register:
                    {
                        OneOf<byte, Error> isr = cpu.StoreRegister(destOperand.Register, source);
                        if (isr.IsT1)
                            return new Error(isr.AsT1, $"Failed to store '{source}' into register '{sourceOperand.Register}' by instruction '{instruction}'", instruction.Position);
                    }
                    break;
                case OperandType.Address:
                    {
                        DataType dataType = WidthToType(instruction.Width);
                        OneOf<byte, Error> asr = cpu.StoreMemory(destOperand.Memory, dataType, source);
                        if (asr.IsT1)
                            return new Error(asr.AsT1, $"Failed to store '{source}' into destination memory '{destOperand.Memory}' with type '{dataType}' by instruction '{instruction}'", instruction.Position);
                    }
                    break;
                default:
                    return new Error(ErrorCode.UnsupportedOperandType, $"The destination operand type '{destOperand.Op}' is not supported by instruction '{instruction}'", instruction.Position);
            }

            int result = instruction.Length;
            return result;
        }
    }
}
