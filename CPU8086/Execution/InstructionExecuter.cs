using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using OneOf;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Final.CPU8086.Execution
{
    public delegate OneOf<int, Error> ExecuteInstructionFunction(CPU cpu, Instruction instruction, IRunState state);

    public class InstructionExecuter
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

        public OneOf<int, Error> Execute(Instruction instruction, IRunState state)
        {
            if (instruction == null)
                return new Error(ErrorCode.MissingInstructionParameter, $"The instruction parameter is missing!", 0);
            InstructionType type = instruction.Mnemonic.Type;
            Contract.Assert((int)type < _typeFunctionTable.Length);
            ExecuteInstructionFunction func = _typeFunctionTable[(int)type];
            if (func == null)
                return new Error(ErrorCode.MissingExecutionFunction, $"No execution function for instruction type '{type}' found!", instruction.Position);
            return func(_cpu, instruction, state);
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

        private static OneOf<byte, Error> StoreValue(CPU cpu, Instruction instruction, IRunState state, InstructionOperand operand, Immediate source)
        {
            switch (operand.Op)
            {
                case OperandType.Register:
                    {
                        OneOf<byte, Error> storeRes = cpu.StoreRegister(instruction, state, operand.Register, source);
                        if (storeRes.IsT1)
                            return new Error(storeRes.AsT1, $"Failed to store '{source}' into register '{operand.Register}' by instruction '{instruction}'", instruction.Position);

                        return storeRes.AsT0;
                    }
                case OperandType.Address:
                    {
                        DataType dataType = WidthToType(instruction.Width);
                        OneOf<byte, Error> storeRes = cpu.StoreMemory(instruction, state, operand.Memory, dataType, source);
                        if (storeRes.IsT1)
                            return new Error(storeRes.AsT1, $"Failed to store '{source}' into destination memory '{operand.Memory}' with type '{dataType}' by instruction '{instruction}'", instruction.Position);
                        return storeRes.AsT0;
                    }
                default:
                    return new Error(ErrorCode.UnsupportedOperandType, $"The destination operand type '{operand.Op}' is not supported by instruction '{instruction}'", instruction.Position);
            }
        }

        public static bool IsParity(byte value)
        {
            uint count = 0;
            for (int i = 0; i < 8; ++i)
            {
                int mask = 1 << i;
                if ((value & mask) == mask)
                    ++count;
            }
            return count % 2 == 0;
        }

        public static bool IsParity(ushort value) => IsParity((byte)(value & 0xFF));

        public static bool IsOverflow8(int value)
            => value > sbyte.MaxValue || value < sbyte.MinValue;
        public static bool IsOverflow16(int value)
            => value > short.MaxValue || value < short.MinValue;

        private static OneOf<int, Error> Add(CPU cpu, Instruction instruction, IRunState state)
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

            bool isZero;
            bool isSign;
            bool isParity;
            bool isOverflow;

            Immediate finalDest;
            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    finalDest = new Immediate((byte)sum, ImmediateFlag.None);
                    isZero = (byte)sum == 0;
                    isSign = (sbyte)sum < 0;
                    isParity = IsParity((byte)sum);
                    isOverflow = IsOverflow8(sum);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)sum, ImmediateFlag.None);
                    isZero = (ushort)sum == 0;
                    isSign = (short)sum < 0;
                    isParity = IsParity((ushort)sum);
                    isOverflow = IsOverflow16(sum);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);

            }

            OneOf<byte, Error> storeRes = StoreValue(cpu, instruction, state, destOperand, finalDest);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            Flags oldFlags = new Flags(
                parity: cpu.Register.ParityFlag ? FlagValue.One : FlagValue.Zero,
                auxiliary: FlagValue.Ignore,
                zero: cpu.Register.ZeroFlag ? FlagValue.One : FlagValue.Zero,
                sign: cpu.Register.SignFlag ? FlagValue.One : FlagValue.Zero,
                trap: FlagValue.Ignore,
                interrupt: FlagValue.Ignore,
                direction: FlagValue.Ignore,
                overflow: cpu.Register.OverflowFlag ? FlagValue.One : FlagValue.Zero
            );

            Flags newFlags = new Flags(
                parity: isParity ? FlagValue.One : FlagValue.Zero,
                auxiliary: FlagValue.Ignore,
                zero: isZero ? FlagValue.One : FlagValue.Zero,
                sign: isSign ? FlagValue.One : FlagValue.Zero,
                trap: FlagValue.Ignore,
                interrupt: FlagValue.Ignore,
                direction: FlagValue.Ignore,
                overflow: isOverflow ? FlagValue.One : FlagValue.Zero
            );

            cpu.Register.ZeroFlag = isZero;
            cpu.Register.SignFlag = isSign;
            cpu.Register.ParityFlag = isParity;
            cpu.Register.OverflowFlag = isOverflow;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

            return 0;
        }

        private static OneOf<int, Error> Sub(CPU cpu, Instruction instruction, IRunState state)
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
            int sub = oldValue - appendValue;

            bool isZero;
            bool isSign;
            bool isParity;
            bool isOverflow;

            Immediate finalDest;
            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    finalDest = new Immediate((byte)sub, ImmediateFlag.None);
                    isZero = (byte)sub == 0;
                    isSign = (sbyte)sub < 0;
                    isParity = IsParity((byte)sub);
                    isOverflow = IsOverflow8(sub);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)sub, ImmediateFlag.None);
                    isZero = (ushort)sub == 0;
                    isSign = (short)sub < 0;
                    isParity = IsParity((ushort)sub);
                    isOverflow = IsOverflow16(sub);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);
            }

            OneOf<byte, Error> storeRes = StoreValue(cpu, instruction, state, destOperand, finalDest);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            Flags oldFlags = new Flags(
                parity: cpu.Register.ParityFlag ? FlagValue.One : FlagValue.Zero,
                auxiliary: FlagValue.Ignore,
                zero: cpu.Register.ZeroFlag ? FlagValue.One : FlagValue.Zero,
                sign: cpu.Register.SignFlag ? FlagValue.One : FlagValue.Zero,
                trap: FlagValue.Ignore,
                interrupt: FlagValue.Ignore,
                direction: FlagValue.Ignore,
                overflow: cpu.Register.OverflowFlag ? FlagValue.One : FlagValue.Zero
            );

            Flags newFlags = new Flags(
                parity: isParity ? FlagValue.One : FlagValue.Zero,
                auxiliary: FlagValue.Ignore,
                zero: isZero ? FlagValue.One : FlagValue.Zero,
                sign: isSign ? FlagValue.One : FlagValue.Zero,
                trap: FlagValue.Ignore,
                interrupt: FlagValue.Ignore,
                direction: FlagValue.Ignore,
                overflow: isOverflow ? FlagValue.One : FlagValue.Zero
            );

            cpu.Register.ZeroFlag = isZero;
            cpu.Register.SignFlag = isSign;
            cpu.Register.ParityFlag = isParity;
            cpu.Register.OverflowFlag = isOverflow;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

            return 0;
        }
        private static OneOf<int, Error> Cmp(CPU cpu, Instruction instruction, IRunState state)
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

            bool isZero;
            bool isSign;
            bool isParity;
            bool isOverflow;

            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    isZero = (byte)cmp == 0;
                    isSign = (sbyte)cmp < 0;
                    isParity = IsParity((byte)cmp);
                    isOverflow = IsOverflow8(cmp);
                    break;

                case DataWidthType.Word:
                    isZero = (ushort)cmp == 0;
                    isSign = (short)cmp < 0;
                    isParity = IsParity((ushort)cmp);
                    isOverflow = IsOverflow16(cmp);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);
            }

            Flags oldFlags = new Flags(
                parity: cpu.Register.ParityFlag ? FlagValue.One : FlagValue.Zero,
                auxiliary: FlagValue.Ignore,
                zero: cpu.Register.ZeroFlag ? FlagValue.One : FlagValue.Zero,
                sign: cpu.Register.SignFlag ? FlagValue.One : FlagValue.Zero,
                trap: FlagValue.Ignore,
                interrupt: FlagValue.Ignore,
                direction: FlagValue.Ignore,
                overflow: cpu.Register.OverflowFlag ? FlagValue.One : FlagValue.Zero
            );

            Flags newFlags = new Flags(
                parity: isParity ? FlagValue.One : FlagValue.Zero,
                auxiliary: FlagValue.Ignore,
                zero: isZero ? FlagValue.One : FlagValue.Zero,
                sign: isSign ? FlagValue.One : FlagValue.Zero,
                trap: FlagValue.Ignore,
                interrupt: FlagValue.Ignore,
                direction: FlagValue.Ignore,
                overflow: isOverflow ? FlagValue.One : FlagValue.Zero
            );

            cpu.Register.ZeroFlag = isZero;
            cpu.Register.SignFlag = isSign;
            cpu.Register.ParityFlag = isParity;
            cpu.Register.OverflowFlag = isOverflow;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

            return 0;
        }

        private static OneOf<int, Error> Mov(CPU cpu, Instruction instruction, IRunState state)
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

            OneOf<byte, Error> storeRes = StoreValue(cpu, instruction, state, destOperand, source);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            return 0;
        }
    }
}
