using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using OneOf;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

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

            _typeFunctionTable[(int)InstructionType.JE] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JNE] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JC] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JNC] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JO] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JNO] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JS] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JNS] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JP] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JNP] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JCXZ] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JG] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JGE] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JL] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JLE] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JA] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JAE] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JB] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.JBE] = ConditionalJump;

            _typeFunctionTable[(int)InstructionType.JMP] = DirectJump;
            _typeFunctionTable[(int)InstructionType.CALL] = DirectJump;
            _typeFunctionTable[(int)InstructionType.RET] = DirectJump;
            _typeFunctionTable[(int)InstructionType.RETF] = DirectJump;

            _typeFunctionTable[(int)InstructionType.LOOP] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.LOOPE] = ConditionalJump;
            _typeFunctionTable[(int)InstructionType.LOOPNZ] = ConditionalJump;

            _typeFunctionTable[(int)InstructionType.PUSH] = PushOrPop;
            _typeFunctionTable[(int)InstructionType.POP] = PushOrPop;

            _typeFunctionTable[(int)InstructionType.PUSHF] = FlagsPushOrPop;
            _typeFunctionTable[(int)InstructionType.POPF] = FlagsPushOrPop;
        }

        public OneOf<int, Error> Execute(Instruction instruction, IRunState state)
        {
            if (instruction == null)
                return new Error(ErrorCode.MissingInstructionParameter, $"The instruction parameter is missing!", 0);
            InstructionType type = instruction.Type;
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
                DataWidthType.QuadWord => DataType.QuadWord,
                _ => DataType.None,
            };
        }

        private static byte WidthToLength(DataWidth width)
        {
            return width.Type switch
            {
                DataWidthType.Byte => 1,
                DataWidthType.Word => 2,
                DataWidthType.DoubleWord => 4,
                DataWidthType.QuadWord => 8,
                _ => 0,
            };
        }

        public static bool IsAuxiliaryOverflow2(int value, int addend) => ((value & 0xF) + (addend & 0xF) > 0xF);
        public static bool IsAuxiliaryUnderflow2(int dividend, int divisor) => ((dividend & 0xF) < (divisor & 0xF));
        public static bool IsAuxiliaryCarryOut2(int multiplicand, int multiplier) => ((multiplicand & 0xF) + (multiplier & 0xF) > 0xF);
        public static bool IsAuxiliaryBorrowIn2(int dividend, int divisor) => ((dividend & 0xF) < (divisor & 0xF));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAuxiliaryOverflow(int value, int addend) => ((value & 0xF) + (addend & 0xF) > 0xF);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAuxiliaryUnderflow(int value, int subtrahend) => (value & 0xF) - (subtrahend & 0xF) < 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsParity8(byte value)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsParity16(ushort value) => IsParity8((byte)(value & 0xFF));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(int value) => value == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCarry8(int value) => value > 0xFF;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCarry8(int a, int b) => a > 0xFF - b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCarry16(int value) => value > 0xFFFF;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCarry16(int a, int b) => a > 0xFFFF - b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBorrow(int value, int subtrahend) => value < subtrahend;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOverflow8(int value) => value > sbyte.MaxValue || value < sbyte.MinValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOverflow16(int value) => value > short.MaxValue || value < short.MinValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSign8(int value) => (value & 0x80) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSign16(int value) => (value & 0x8000) != 0;

        private static OneOf<Immediate, Error> LoadOperand(CPU cpu, Instruction instruction, InstructionOperand operand)
        {
            Immediate result;
            switch (operand.Type)
            {
                case OperandType.Register:
                case OperandType.Accumulator:
                case OperandType.Segment:
                    {
                        OneOf<Immediate, Error> ilr = cpu.LoadRegister(operand.Register);
                        if (ilr.IsT1)
                            return new Error(ilr.AsT1, $"Failed to load source register '{operand.Register}' by instruction '{instruction}'", instruction.Position);
                        result = ilr.AsT0;
                    }
                    break;
                case OperandType.Memory:
                    {
                        DataType dataType = WidthToType(instruction.Width);
                        OneOf<Immediate, Error> alr = cpu.LoadMemory(operand.Memory, dataType);
                        if (alr.IsT1)
                            return new Error(alr.AsT1, $"Failed to load source memory '{operand.Memory}' with type '{dataType}' by instruction '{instruction}'", instruction.Position);
                        result = alr.AsT0;
                    }
                    break;
                case OperandType.Immediate:
                    result = operand.Immediate;
                    break;

                case OperandType.Value:
                    result = new Immediate(operand.Value);
                    break;

                default:
                    return new Error(ErrorCode.UnsupportedOperandType, $"The source operand type '{operand.Type}' is not supported by instruction '{instruction}'", instruction.Position);
            }
            return result;
        }

        private static OneOf<byte, Error> StoreOperand(CPU cpu, Instruction instruction, IRunState state, InstructionOperand operand, Immediate source)
        {
            switch (operand.Type)
            {
                case OperandType.Register:
                case OperandType.Accumulator:
                case OperandType.Segment:
                    {
                        OneOf<byte, Error> storeRes = cpu.StoreRegister(instruction, state, operand.Register, source);
                        if (storeRes.IsT1)
                            return new Error(storeRes.AsT1, $"Failed to store '{source}' into register '{operand.Register}' by instruction '{instruction}'", instruction.Position);

                        return storeRes.AsT0;
                    }
                case OperandType.Memory:
                    {
                        DataType dataType = WidthToType(instruction.Width);
                        OneOf<byte, Error> storeRes = cpu.StoreMemory(instruction, state, operand.Memory, dataType, source);
                        if (storeRes.IsT1)
                            return new Error(storeRes.AsT1, $"Failed to store '{source}' into destination memory '{operand.Memory}' with type '{dataType}' by instruction '{instruction}'", instruction.Position);
                        return storeRes.AsT0;
                    }
                default:
                    return new Error(ErrorCode.UnsupportedOperandType, $"The destination operand type '{operand.Type}' is not supported by instruction '{instruction}'", instruction.Position);
            }
        }

        private static OneOf<int, Error> PushToStack(CPU cpu, Instruction instruction, IRunState state, DataWidth width, Immediate value)
        {
            DataType dataType = WidthToType(width);
            byte dataLen = WidthToLength(width);
            if (dataType == DataType.None || dataLen == 0)
                return new Error(ErrorCode.UnsupportedDataWidth, $"Unsupported data width '{width}' for push instruction '{instruction.Mnemonic}'", instruction.Position);

            // Decrement SP
            Immediate oldSP = new Immediate(cpu.Register.SP);
            cpu.Register.SP -= dataLen;
            Immediate newSP = new Immediate(cpu.Register.SP);
            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(RegisterType.SP, oldSP), new ExecutedValue(RegisterType.SP, newSP))));

            MemoryAddress address = new MemoryAddress(EffectiveAddressCalculation.DirectAddress, new Immediate(cpu.Register.SP), SegmentType.SS, 0);

            // Load previous value from SS:SP (Only used for IRunState)
            OneOf<Immediate, Error> previousLoadRes = cpu.LoadMemory(address, dataType);
            if (previousLoadRes.IsT1)
                return previousLoadRes.AsT1;
            Immediate previousValue = previousLoadRes.AsT0;

            // Store value in SS:SP
            OneOf<byte, Error> storeRes = cpu.StoreMemory(instruction, state, address, dataType, value);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(address, previousValue), new ExecutedValue(address, value))));

            return 0;
        }

        private static OneOf<Immediate, Error> PopFromStack(CPU cpu, Instruction instruction, IRunState state, DataWidth width)
        {
            DataType dataType = WidthToType(width);
            byte dataLen = WidthToLength(width);
            if (dataType == DataType.None || dataLen == 0)
                return new Error(ErrorCode.UnsupportedDataWidth, $"Unsupported data width '{width}' for push instruction '{instruction.Mnemonic}'", instruction.Position);

            Immediate oldSP = new Immediate(cpu.Register.SP);

            // Load value from SS:SP
            OneOf<Immediate, Error> loadRes = cpu.LoadMemory(new MemoryAddress(EffectiveAddressCalculation.DirectAddress, new Immediate(oldSP.S16), SegmentType.SS, 0), dataType);
            if (loadRes.IsT1)
                return loadRes.AsT1;
            Immediate loadedValue = loadRes.AsT0;

            // Increment SP
            cpu.Register.SP += dataLen;
            Immediate newSP = new Immediate(cpu.Register.SP);
            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(RegisterType.SP, oldSP), new ExecutedValue(RegisterType.SP, newSP))));

            return loadedValue;
        }

        // Implements all flags related instruction types, such as PUSHF/POPF
        private static OneOf<int, Error> FlagsPushOrPop(CPU cpu, Instruction instruction, IRunState state)
        {
            switch (instruction.Type)
            {
                case InstructionType.PUSHF:
                    return PushToStack(cpu, instruction, state, DataWidth.Word, new Immediate(cpu.Register.Status)); 

                case InstructionType.POPF:
                    OneOf<Immediate, Error> popRes = PopFromStack(cpu, instruction, state, DataWidth.Word);
                    if (popRes.IsT1)
                        return popRes.AsT1;

                    ushort newStatus = popRes.AsT0.U16;
                    ushort oldStatus = cpu.Register.Status;

                    FlagsDefinition oldFlags = new FlagsDefinition(oldStatus);
                    FlagsDefinition newFlags = new FlagsDefinition(newStatus);

                    cpu.Register.Status = newStatus;

                    state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

                    return 0;

                default:
                    throw new NotSupportedException($"The popRes instruction '{instruction.Type}' is not supported");
            }
        }

        private static OneOf<int, Error> PushOrPop(CPU cpu, Instruction instruction, IRunState state)
        {
            const int expectedOperandLen = 1;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for push instruction '{instruction.Mnemonic}'", instruction.Position);

            switch (instruction.Type)
            {
                case InstructionType.PUSH:
                case InstructionType.POP:
                    {
                        InstructionOperand op = instruction.Operands[0];
                        if (!(op.Type == OperandType.Register || op.Type == OperandType.Accumulator || op.Type == OperandType.Segment))
                            return new Error(ErrorCode.MismatchInstructionOperands, $"Expect operand type '{OperandType.Register}' or '{OperandType.Accumulator}' or '{OperandType.Segment}', but got '{op.Type}' for push instruction '{instruction.Mnemonic}'", instruction.Position);

                        DataType dataType = WidthToType(instruction.Width);
                        byte dataLen = WidthToLength(instruction.Width);
                        if (dataType == DataType.None || dataLen == 0)
                            return new Error(ErrorCode.UnsupportedDataWidth, $"Unsupported data width '{instruction.Width}' for push instruction '{instruction.Mnemonic}'", instruction.Position);

                        RegisterType register = op.Register;

                        // Load value from register
                        OneOf<Immediate, Error> regLoad = cpu.LoadRegister(register);
                        if (regLoad.IsT1)
                            return regLoad.AsT1;
                        Immediate regValue = regLoad.AsT0;

                        if (instruction.Type == InstructionType.PUSH)
                        {
                            OneOf<int, Error> pushRes = PushToStack(cpu, instruction, state, instruction.Width, regValue);
                            if (pushRes.IsT1)
                                return pushRes.AsT1;
                        }
                        else
                        {
                            Contract.Assert(instruction.Type == InstructionType.POP);

                            OneOf<Immediate, Error> popRes = PopFromStack(cpu, instruction, state, instruction.Width);
                            if (popRes.IsT1)
                                return popRes.AsT1;
                            Immediate newValue = popRes.AsT0;

                            // Store value to register
                            OneOf<byte, Error> storeRes = cpu.StoreRegister(instruction, state, register, newValue);
                            if (storeRes.IsT1)
                                return storeRes.AsT1;

                            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(register, regValue), new ExecutedValue(register, newValue))));
                        }
                    }
                    break;
            }

            return 0;
        }

        private static OneOf<int, Error> DirectJump(CPU cpu, Instruction instruction, IRunState state)
        {
            Contract.Assert(instruction != null);

            InstructionType type = instruction.Type;

            switch (type)
            {
                case InstructionType.JMP:
                    break; // Nothing special todo, return operand address

                case InstructionType.CALL:
                    {
                        Immediate sp = new Immediate(cpu.Register.SP);
                        OneOf<int, Error> pushRes = PushToStack(cpu, instruction, state, DataWidth.Word, sp);
                        if (pushRes.IsT1)
                            return pushRes.AsT1;
                        // NOTE(final): Jump is done below
                    }
                    break;

                case InstructionType.RET:
                    {
                        OneOf<Immediate, Error> popRes = PopFromStack(cpu, instruction, state, DataWidth.Word);
                        if (popRes.IsT1)
                            return popRes.AsT1;

                        // TODO(final): Not sure if this is correct
                        return popRes.AsT0.Value;
                    }

                case InstructionType.RETF:
                    throw new NotImplementedException();

                default:
                    return new Error(ErrorCode.UnsupportedInstruction, $"Unsupported type '{type}' for direct jump instruction '{instruction.Mnemonic}'", instruction.Position);
            }

            const int expectedOperandLen = 1;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for direct jump instruction '{instruction.Mnemonic}'", instruction.Position);

            InstructionOperand labelOperand = instruction.Operands[0];

            OneOf<Immediate, Error> loadRes = LoadOperand(cpu, instruction, labelOperand);
            if (loadRes.IsT1)
                return loadRes.AsT1;
            int relativeAddress = loadRes.AsT0.Value;
            return relativeAddress;
        }

        private static OneOf<int, Error> ConditionalJump(CPU cpu, Instruction instruction, IRunState state)
        {
            Contract.Assert(instruction != null);

            const int expectedOperandLen = 1;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for conditional jump instruction '{instruction.Mnemonic}'", instruction.Position);

            InstructionType type = instruction.Type;

            InstructionOperand labelOperand = instruction.Operands[0];

            bool isZero = cpu.Register.ZeroFlag;
            bool isCarry = cpu.Register.CarryFlag;
            bool isOverflow = cpu.Register.OverflowFlag;
            bool isSign = cpu.Register.SignFlag;
            bool isParity = cpu.Register.ParityFlag;

            // Load CX if needed
            short cx;
            switch (type)
            {
                case InstructionType.JCXZ:
                case InstructionType.LOOP:
                case InstructionType.LOOPE:
                case InstructionType.LOOPNZ:
                    {
                        OneOf<Immediate, Error> cxLoad = cpu.LoadRegister(RegisterType.CX);
                        if (cxLoad.IsT1)
                            return cxLoad.AsT1;
                        cx = cxLoad.AsT0.S16;
                    }
                    break;

                default:
                    cx = 0;
                    break;
            }
            short initialCX = cx;

            // Based on Table 2-15 (Page 61)
            bool canJump;
            switch (type)
            {
                // Jump if Above, Jump if Not Below or Equal (ZF == 0 && CF == 0)
                case InstructionType.JA:
                    canJump = (isZero || isCarry) == false; break;
                // Jump if Above or Equal, Jump if Not Below (CF == 0)
                case InstructionType.JAE:
                    canJump = !isCarry; break;
                // Jump if Below, Jump if Not Above or Equal (CF == 1)
                case InstructionType.JB:
                    canJump = isCarry; break;
                // Jump if Below or Equal, Jump if Not Above  (ZF == 1 || CF == 1)
                case InstructionType.JBE:
                    canJump = (isZero || isCarry) == true; break;
                // Jump if Carry (CF == 1)
                case InstructionType.JC:
                    canJump = isCarry; break;
                // Jump if Zero, Jump if Equal (ZF == 1)
                case InstructionType.JE:
                    canJump = isZero; break;
                // Jump if Greater, Jump if Not Less or Equal (ZF == 0 && SF == OF)
                case InstructionType.JG:
                    canJump = ((isSign ^ isOverflow) | isZero) == false; break;
                // Jump if Greater or Equal, Jump if Not Less (SF == OF)
                case InstructionType.JGE:
                    canJump = (isSign ^ isOverflow) == false; break;
                // Jump if Less, Jump if Not Greater or Equal (SF != OF)
                case InstructionType.JL:
                    canJump = (isSign ^ isOverflow) == true; break;
                // Jump if Less or Equal, Jump if Not Greater (ZF == 1 || SF != OF)
                case InstructionType.JLE:
                    canJump = ((isSign ^ isOverflow) | isZero) == true; break;
                // Jump if No Carry (CF == 0)
                case InstructionType.JNC:
                    canJump = !isCarry; break;
                // Jump if Not Zero, Jump if Not Equal (ZF == 0)
                case InstructionType.JNE:
                    canJump = !isZero; break;
                // Jump if No Overflow (OF == 0)
                case InstructionType.JNO:
                    canJump = !isOverflow; break;
                // Jump if Not Parity, Jump if Parity is Odd (PF == 0)
                case InstructionType.JNP:
                    canJump = !isParity; break;
                // Jump if No Signed (SF == 0)
                case InstructionType.JNS:
                    canJump = !isSign; break;
                // Jump if Overflow (OF == 1)
                case InstructionType.JO:
                    canJump = isOverflow; break;
                // Jump if Parity, Jump if Parity is Even (PF == 1)
                case InstructionType.JP:
                    canJump = isParity; break;
                // Jump if Signed (SF == 1)
                case InstructionType.JS:
                    canJump = isSign; break;

                // Jump if CX Register is zero (CX == 0)
                case InstructionType.JCXZ:
                    canJump = cx == 0; break;

                // Decrement CX and Loop if CX != 0
                case InstructionType.LOOP:
                    {
                        --cx;
                        canJump = cx != 0;
                    }
                    break;

                // Decrement CX and Loop if CX != 0 && ZF == 1
                case InstructionType.LOOPE:
                    {
                        --cx;
                        canJump = cx != 0 && isZero;
                    }
                    break;

                // Decrement CX and Loop if CX == 0 && ZF == 0
                case InstructionType.LOOPNZ:
                    {
                        --cx;
                        canJump = cx != 0 && !isZero;
                    }
                    break;

                default:
                    return new Error(ErrorCode.UnsupportedInstruction, $"Unsupported type '{type}' for conditional jump instruction '{instruction.Mnemonic}'", instruction.Position);
            }

            // Update CX if needed
            if (initialCX != cx)
            {
                OneOf<byte, Error> storeRes = cpu.StoreRegister(instruction, state, RegisterType.CX, new Immediate(cx));
                if (storeRes.IsT1)
                    return storeRes.AsT1;
            }

            if (canJump)
            {
                OneOf<Immediate, Error> loadRes = LoadOperand(cpu, instruction, labelOperand);
                if (loadRes.IsT1)
                    return loadRes.AsT1;
                int relativeAddress = loadRes.AsT0.Value;
                return relativeAddress;
            }

            return 0;
        }

        private delegate int ArithmeticOperationHandler(int a, int b);

        enum AuxiliaryState
        {
            None = 0,
            Overflow,
            Underflow,
        }

        private static OneOf<int, Error> ArithmeticOperation(CPU cpu, Instruction instruction, IRunState state, FlagsDefinition flags, ArithmeticOperationHandler operation, AuxiliaryState overflow, ReadOnlySpan<char> instructionName)
        {
            Contract.Assert(instruction != null);

            const int expectedOperandLen = 2;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for {instructionName} instruction '{instruction.Mnemonic}'", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadOperand(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;

            Immediate source = sourceRes.AsT0;

            OneOf<Immediate, Error> previosDestRes = LoadOperand(cpu, instruction, destOperand);
            if (previosDestRes.IsT1)
                return previosDestRes.AsT1;

            Immediate initial = previosDestRes.AsT0;

            int a = initial.Value;
            int b = source.Value;

            int result = operation.Invoke(a, b);

            bool isAuxiliary;
            if (overflow == AuxiliaryState.Overflow)
                isAuxiliary = IsAuxiliaryOverflow(a, b);
            else if (overflow == AuxiliaryState.Underflow)
                isAuxiliary = IsAuxiliaryUnderflow(a, b);
            else
                isAuxiliary = false;

            bool isZero = result == 0;
            bool isSign;
            bool isCarry;
            bool isParity;
            bool isOverflow;
            Immediate finalDest;
            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    finalDest = new Immediate((byte)result);
                    isSign = IsSign8(result);
                    isCarry = IsCarry8(a, b);
                    isParity = IsParity8((byte)result);
                    isOverflow = IsOverflow8((byte)result);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)result);
                    isSign = IsSign16(result);
                    isCarry = IsCarry16(a, b);
                    isParity = IsParity16((byte)result);
                    isOverflow = IsOverflow16((byte)result);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);
            }

            OneOf<byte, Error> storeRes = StoreOperand(cpu, instruction, state, destOperand, finalDest);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            ushort status = cpu.Register.Status;

            FlagsDefinition oldFlags = new FlagsDefinition(status);

            FlagsDefinition newFlags = new FlagsDefinition(
                carry: isCarry ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                parity: isParity ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                auxiliary: isAuxiliary ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                zero: isZero ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                sign: isSign ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                trap: FlagDefinitionValue.Ignore,
                interrupt: FlagDefinitionValue.Ignore,
                direction: FlagDefinitionValue.Ignore,
                overflow: isOverflow ? FlagDefinitionValue.One : FlagDefinitionValue.Zero
            );

            cpu.Register.CarryFlag = isCarry;
            cpu.Register.AuxiliaryCarryFlag = isAuxiliary;
            cpu.Register.ParityFlag = isParity;
            cpu.Register.ZeroFlag = isZero;
            cpu.Register.SignFlag = isSign;
            cpu.Register.OverflowFlag = isOverflow;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

            return 0;
        }

        private static OneOf<int, Error> Add(CPU cpu, Instruction instruction, IRunState state)
        {
            Contract.Assert(instruction != null);

            const int expectedOperandLen = 2;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for add instruction '{instruction.Mnemonic}'", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadOperand(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;
            Immediate source = sourceRes.AsT0;

            OneOf<Immediate, Error> previosDestRes = LoadOperand(cpu, instruction, destOperand);
            if (previosDestRes.IsT1)
                return previosDestRes.AsT1;
            Immediate previosDest = previosDestRes.AsT0;

            int value = previosDest.Value;
            int addend = source.Value;
            int sum = value + addend;

            bool isAuxiliary = IsAuxiliaryOverflow(value, addend);
            bool isZero = IsZero(sum);
            bool isCarry;
            bool isParity;
            bool isSign;
            bool isOverflow;

            Immediate finalDest;
            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    finalDest = new Immediate((byte)sum);
                    isCarry = IsCarry8(sum);
                    isParity = IsParity8((byte)sum);
                    isSign = IsSign8(sum);
                    isOverflow = IsOverflow8(sum);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)sum);
                    isCarry = IsCarry16(sum);
                    isParity = IsParity16((ushort)sum);
                    isSign = IsSign16(sum);
                    isOverflow = IsOverflow16(sum);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);

            }

            OneOf<byte, Error> storeRes = StoreOperand(cpu, instruction, state, destOperand, finalDest);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            FlagsDefinition oldFlags = new FlagsDefinition(
                carry: cpu.Register.CarryFlag ? FlagDefinitionValue.One : Types.FlagDefinitionValue.Zero,
                parity: cpu.Register.ParityFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                auxiliary: cpu.Register.AuxiliaryCarryFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                zero: cpu.Register.ZeroFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                sign: cpu.Register.SignFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                trap: FlagDefinitionValue.Ignore,
                interrupt: FlagDefinitionValue.Ignore,
                direction: FlagDefinitionValue.Ignore,
                overflow: cpu.Register.OverflowFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero
            );

            FlagsDefinition newFlags = new FlagsDefinition(
                carry: isCarry ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                parity: isParity ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                auxiliary: isAuxiliary ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                zero: isZero ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                sign: isSign ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                trap: FlagDefinitionValue.Ignore,
                interrupt: FlagDefinitionValue.Ignore,
                direction: FlagDefinitionValue.Ignore,
                overflow: isOverflow ? FlagDefinitionValue.One : FlagDefinitionValue.Zero
            );

            cpu.Register.CarryFlag = isCarry;
            cpu.Register.AuxiliaryCarryFlag = isAuxiliary;
            cpu.Register.ParityFlag = isParity;
            cpu.Register.ZeroFlag = isZero;
            cpu.Register.SignFlag = isSign;
            cpu.Register.OverflowFlag = isOverflow;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

            return 0;
        }

        private static OneOf<int, Error> Sub(CPU cpu, Instruction instruction, IRunState state)
        {
            Contract.Assert(instruction != null);

            const int expectedOperandLen = 2;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for sub instruction '{instruction.Mnemonic}'", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadOperand(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;
            Immediate source = sourceRes.AsT0;

            OneOf<Immediate, Error> previosDestRes = LoadOperand(cpu, instruction, destOperand);
            if (previosDestRes.IsT1)
                return previosDestRes.AsT1;
            Immediate previosDest = previosDestRes.AsT0;

            int value = previosDest.Value;
            int subtrahend = source.Value;
            int difference = value - subtrahend;

            bool isCarry = IsBorrow(value, subtrahend);
            bool isAuxiliary = IsAuxiliaryUnderflow(value, subtrahend);
            bool isZero = IsZero(difference);
            bool isParity;
            bool isSign;
            bool isOverflow;

            Immediate finalDest;
            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    finalDest = new Immediate((byte)difference);
                    isParity = IsParity8((byte)difference);
                    isSign = IsSign8(difference);
                    isOverflow = IsOverflow8(difference);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)difference);
                    isParity = IsParity16((ushort)difference);
                    isSign = IsSign16(difference);
                    isOverflow = IsOverflow16(difference);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);
            }

            OneOf<byte, Error> storeRes = StoreOperand(cpu, instruction, state, destOperand, finalDest);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            FlagsDefinition oldFlags = new FlagsDefinition(
                carry: cpu.Register.CarryFlag ? FlagDefinitionValue.One : Types.FlagDefinitionValue.Zero,
                parity: cpu.Register.ParityFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                auxiliary: cpu.Register.AuxiliaryCarryFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                zero: cpu.Register.ZeroFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                sign: cpu.Register.SignFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                trap: FlagDefinitionValue.Ignore,
                interrupt: FlagDefinitionValue.Ignore,
                direction: FlagDefinitionValue.Ignore,
                overflow: cpu.Register.OverflowFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero
            );

            FlagsDefinition newFlags = new FlagsDefinition(
                carry: isCarry ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                parity: isParity ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                auxiliary: isAuxiliary ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                zero: isZero ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                sign: isSign ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                trap: FlagDefinitionValue.Ignore,
                interrupt: FlagDefinitionValue.Ignore,
                direction: FlagDefinitionValue.Ignore,
                overflow: isOverflow ? FlagDefinitionValue.One : FlagDefinitionValue.Zero
            );

            cpu.Register.CarryFlag = isCarry;
            cpu.Register.AuxiliaryCarryFlag = isAuxiliary;
            cpu.Register.ParityFlag = isParity;
            cpu.Register.ZeroFlag = isZero;
            cpu.Register.SignFlag = isSign;
            cpu.Register.OverflowFlag = isOverflow;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

            return 0;
        }

        private static OneOf<int, Error> Cmp(CPU cpu, Instruction instruction, IRunState state)
        {
            Contract.Assert(instruction != null);

            const int expectedOperandLen = 2;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for cmp instruction '{instruction.Mnemonic}'", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadOperand(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;
            Immediate source = sourceRes.AsT0;

            OneOf<Immediate, Error> destRes = LoadOperand(cpu, instruction, destOperand);
            if (destRes.IsT1)
                return destRes.AsT1;
            Immediate previosDest = destRes.AsT0;

            int value = previosDest.Value;
            int subtrahend = source.Value;
            int difference = value - subtrahend;

            bool isCarry = IsBorrow(value, subtrahend);
            bool isAuxiliary = IsAuxiliaryUnderflow(value, subtrahend);
            bool isZero = IsZero(difference);
            bool isParity;
            bool isSign;
            bool isOverflow;

            switch (instruction.Width.Type)
            {
                case DataWidthType.Byte:
                    isParity = IsParity8((byte)difference);
                    isSign = IsSign8(difference);
                    isOverflow = IsOverflow8(difference);
                    break;

                case DataWidthType.Word:
                    isParity = IsParity16((ushort)difference);
                    isSign = IsSign16(difference);
                    isOverflow = IsOverflow16(difference);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);
            }

            FlagsDefinition oldFlags = new FlagsDefinition(
                carry: cpu.Register.CarryFlag ? FlagDefinitionValue.One : Types.FlagDefinitionValue.Zero,
                parity: cpu.Register.ParityFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                auxiliary: cpu.Register.AuxiliaryCarryFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                zero: cpu.Register.ZeroFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                sign: cpu.Register.SignFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                trap: FlagDefinitionValue.Ignore,
                interrupt: FlagDefinitionValue.Ignore,
                direction: FlagDefinitionValue.Ignore,
                overflow: cpu.Register.OverflowFlag ? FlagDefinitionValue.One : FlagDefinitionValue.Zero
            );

            FlagsDefinition newFlags = new FlagsDefinition(
                carry: isCarry ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                parity: isParity ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                auxiliary: isAuxiliary ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                zero: isZero ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                sign: isSign ? FlagDefinitionValue.One : FlagDefinitionValue.Zero,
                trap: FlagDefinitionValue.Ignore,
                interrupt: FlagDefinitionValue.Ignore,
                direction: FlagDefinitionValue.Ignore,
                overflow: isOverflow ? FlagDefinitionValue.One : FlagDefinitionValue.Zero
            );

            cpu.Register.CarryFlag = isCarry;
            cpu.Register.AuxiliaryCarryFlag = isAuxiliary;
            cpu.Register.ParityFlag = isParity;
            cpu.Register.ZeroFlag = isZero;
            cpu.Register.SignFlag = isSign;
            cpu.Register.OverflowFlag = isOverflow;

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(oldFlags), new ExecutedValue(newFlags))));

            return 0;
        }

        private static OneOf<int, Error> Mov(CPU cpu, Instruction instruction, IRunState state)
        {
            Contract.Assert(instruction != null);

            const int expectedOperandLen = 2;
            if (instruction.Operands.Length != expectedOperandLen)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Expect '{expectedOperandLen}' operands, but got '{instruction.Operands.Length}' for move instruction '{instruction.Mnemonic}'", instruction.Position);

            InstructionOperand destOperand = instruction.Operands[0];

            InstructionOperand sourceOperand = instruction.Operands[1];

            OneOf<Immediate, Error> sourceRes = LoadOperand(cpu, instruction, sourceOperand);
            if (sourceRes.IsT1)
                return sourceRes.AsT1;

            Immediate source = sourceRes.AsT0;

            OneOf<byte, Error> storeRes = StoreOperand(cpu, instruction, state, destOperand, source);
            if (storeRes.IsT1)
                return storeRes.AsT1;

            return 0;
        }
    }
}
