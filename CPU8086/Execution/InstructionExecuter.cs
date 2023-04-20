using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using OneOf;
using System;
using System.Diagnostics.Contracts;

namespace Final.CPU8086.Execution
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
            _typeFunctionTable[(int)InstructionType.ADD] = Add;
            _typeFunctionTable[(int)InstructionType.SUB] = Sub;
            _typeFunctionTable[(int)InstructionType.CMP] = Cmp;
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

        private static OneOf<Immediate, Error> LoadValue(CPU cpu, Instruction instruction, InstructionOperand operand)
        {
            Immediate result;
            switch (operand.Op)
            {
                case OperandType.Register:
                    {
                        OneOf<Immediate, Error> ilr = cpu.LoadRegister(operand.Register);
                        if (ilr.IsT1)
                            return new Error(ilr.AsT1, $"Failed to load source register '{operand.Register}' by instruction '{instruction}'", instruction.Position);
                        result = ilr.AsT0;
                    }
                    break;
                case OperandType.Address:
                    {
                        DataType dataType = WidthToType(instruction.Width);
                        OneOf<Immediate, Error> alr = cpu.LoadMemory(operand.Memory, dataType);
                        if (alr.IsT1)
                            return new Error(alr.AsT1, $"Failed to load source memory '{operand.Memory}' with type '{dataType}' by instruction '{instruction}'", instruction.Position);
                        result = alr.AsT0;
                    }
                    break;
                case OperandType.Immediate:
                case OperandType.Value:
                    result = operand.Immediate;
                    break;
                default:
                    return new Error(ErrorCode.UnsupportedOperandType, $"The source operand type '{operand.Op}' is not supported by instruction '{instruction}'", instruction.Position);
            }
            return result;
        }

        private static OneOf<byte, Error> StoreValue(CPU cpu, Instruction instruction, InstructionOperand operand, Immediate source)
        {
            switch (operand.Op)
            {
                case OperandType.Register:
                    {
                        OneOf<byte, Error> isr = cpu.StoreRegister(operand.Register, source);
                        if (isr.IsT1)
                            return new Error(isr.AsT1, $"Failed to store '{source}' into register '{operand.Register}' by instruction '{instruction}'", instruction.Position);
                        return isr.AsT0;
                    }
                case OperandType.Address:
                    {
                        DataType dataType = WidthToType(instruction.Width);
                        OneOf<byte, Error> asr = cpu.StoreMemory(operand.Memory, dataType, source);
                        if (asr.IsT1)
                            return new Error(asr.AsT1, $"Failed to store '{source}' into destination memory '{operand.Memory}' with type '{dataType}' by instruction '{instruction}'", instruction.Position);
                        return asr.AsT0;
                    }
                default:
                    return new Error(ErrorCode.UnsupportedOperandType, $"The destination operand type '{operand.Op}' is not supported by instruction '{instruction}'", instruction.Position);
            }
        }

        private static OneOf<int, Error> Add(CPU cpu, Instruction instruction)
        {
            Contract.Assert(instruction != null);

            if (instruction.Operands.Length != 2)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Invalid number of operands for {instruction.Mnemonic} instruction", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadValue(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;
            Immediate source = sourceRes.AsT0;

            OneOf<Immediate, Error> previosDestRes = LoadValue(cpu, instruction, destOperand);
            if (previosDestRes.IsT1)
                return previosDestRes.AsT1;
            Immediate previosDest = previosDestRes.AsT0;

            int oldValue = previosDest.Value;
            int appendValue = source.Value;
            int sum = oldValue + appendValue;

            Immediate finalDest;
            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    finalDest = new Immediate((byte)sum, ImmediateFlag.None);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)sum, ImmediateFlag.None);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);

            }

            OneOf<byte, Error> storeRes = StoreValue(cpu, instruction, destOperand, finalDest);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            cpu.Register.ZeroFlag = sum == 0;

            int result = instruction.Length;

            return result;
        }

        private static OneOf<int, Error> Sub(CPU cpu, Instruction instruction)
        {
            Contract.Assert(instruction != null);

            if (instruction.Operands.Length != 2)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Invalid number of operands for {instruction.Mnemonic} instruction", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadValue(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;
            Immediate source = sourceRes.AsT0;

            OneOf<Immediate, Error> previosDestRes = LoadValue(cpu, instruction, destOperand);
            if (previosDestRes.IsT1)
                return previosDestRes.AsT1;
            Immediate previosDest = previosDestRes.AsT0;

            int oldValue = previosDest.Value;
            int appendValue = source.Value;
            int sum = oldValue - appendValue;

            Immediate finalDest;
            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    finalDest = new Immediate((byte)sum, ImmediateFlag.None);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)sum, ImmediateFlag.None);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);
            }

            OneOf<byte, Error> storeRes = StoreValue(cpu, instruction, destOperand, finalDest);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            cpu.Register.ZeroFlag = sum == 0;

            int result = instruction.Length;

            return result;
        }
        private static OneOf<int, Error> Cmp(CPU cpu, Instruction instruction)
        {
            Contract.Assert(instruction != null);

            if (instruction.Operands.Length != 2)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Invalid number of operands for {instruction.Mnemonic} instruction", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadValue(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;
            Immediate source = sourceRes.AsT0;

            OneOf<Immediate, Error> destRes = LoadValue(cpu, instruction, destOperand);
            if (destRes.IsT1)
                return destRes.AsT1;
            Immediate previosDest = destRes.AsT0;

            int oldValue = previosDest.Value;
            int appendValue = source.Value;
            int cmp = oldValue - appendValue;

            cpu.Register.ZeroFlag = cmp == 0;

            int result = instruction.Length;

            return result;
        }

        private static OneOf<int, Error> Mov(CPU cpu, Instruction instruction)
        {
            Contract.Assert(instruction != null);

            if (instruction.Operands.Length != 2)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Invalid number of operands for {instruction.Mnemonic} instruction", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadValue(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;

            Immediate source = sourceRes.AsT0;

            OneOf<byte, Error> storeRes = StoreValue(cpu, instruction, destOperand, source);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            int result = instruction.Length;

            return result;
        }
    }
}
