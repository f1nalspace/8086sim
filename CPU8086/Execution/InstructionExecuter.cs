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
            _typeFunctionTable[(int)InstructionType.PUSHF] = PushOrPop;
            _typeFunctionTable[(int)InstructionType.POPF] = PushOrPop;
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

        private static OneOf<Immediate, Error> LoadOperand(CPU cpu, Instruction instruction, InstructionOperand operand)
        {
            Immediate result;
            switch (operand.Type)
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
                    return new Error(ErrorCode.UnsupportedOperandType, $"The destination operand type '{operand.Type}' is not supported by instruction '{instruction}'", instruction.Position);
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
                        const OperandType expectedOperandType = OperandType.Register;
                        InstructionOperand op = instruction.Operands[0];
                        if (op.Type != expectedOperandType)
                            return new Error(ErrorCode.MismatchInstructionOperands, $"Expect operand type '{expectedOperandType}', but got '{op.Type}' for push instruction '{instruction.Mnemonic}'", instruction.Position);

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
                            // Decrement SP
                            Immediate oldSP = new Immediate(cpu.Register.SP);
                            cpu.Register.SP -= dataLen;
                            Immediate newSP = new Immediate(cpu.Register.SP);
                            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(RegisterType.SP, oldSP), new ExecutedValue(RegisterType.SP, newSP))));

                            MemoryAddress address = new MemoryAddress(EffectiveAddressCalculation.DirectAddress, cpu.Register.SP, SegmentType.SS, 0);

                            // Load previous value from SS:SP
                            OneOf<Immediate, Error> previousLoadRes = cpu.LoadMemory(address, dataType);
                            if (previousLoadRes.IsT1)
                                return previousLoadRes.AsT1;
                            Immediate previousValue = previousLoadRes.AsT0;

                            // Store register in SS:SP
                            OneOf<byte, Error> storeRes = cpu.StoreMemory(instruction, state, address, dataType, regValue);
                            if (storeRes.IsT1)
                                return storeRes.AsT1;

                            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(address, previousValue), new ExecutedValue(address, regValue))));
                        }
                        else
                        {
                            Contract.Assert(instruction.Type == InstructionType.POP);

                            Immediate oldSP = new Immediate(cpu.Register.SP);

                            // Load register value from SS:SP
                            OneOf<Immediate, Error> loadRes = cpu.LoadMemory(new MemoryAddress(EffectiveAddressCalculation.DirectAddress, oldSP.S16, SegmentType.SS, 0), dataType);
                            if (loadRes.IsT1)
                                return loadRes.AsT1;
                            Immediate newValue = loadRes.AsT0;

                            // Increment SP
                            cpu.Register.SP += dataLen;
                            Immediate newSP = new Immediate(cpu.Register.SP);
                            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(RegisterType.SP, oldSP), new ExecutedValue(RegisterType.SP, newSP))));

                            // Store value to register
                            OneOf<byte, Error> storeRes = cpu.StoreRegister(instruction, state, register, newValue);
                            if (storeRes.IsT1)
                                return storeRes.AsT1;

                            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(register, regValue), new ExecutedValue(register, newValue))));
                        }
                    } break;

                case InstructionType.PUSHF:
                    throw new NotImplementedException();

                case InstructionType.POPF:
                    throw new NotImplementedException();
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
                    // TODO(final): PUSH IP to the stack and return operand address
                    throw new NotImplementedException();

                case InstructionType.RET:
                case InstructionType.RETF:
                    // TODO(final): POP IP from the stack and return that address
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

            bool canJump;
            switch (type)
            {
                // Jump if Zero, Jump if Equal (ZF == 1)
                case InstructionType.JE:
                    canJump = isZero; break;
                // Jump if Not Zero, Jump if Not Equal (ZF == 0)
                case InstructionType.JNE:
                    canJump = !isZero; break;
                // Jump if Carry (CF == 1)
                case InstructionType.JC:
                    canJump = isCarry; break;
                // Jump if No Carry (CF == 0)
                case InstructionType.JNC:
                    canJump = !isCarry; break;
                // Jump if Overflow (OF == 1)
                case InstructionType.JO:
                    canJump = isOverflow; break;
                // Jump if No Overflow (OF == 0)
                case InstructionType.JNO:
                    canJump = !isOverflow; break;
                // Jump if Signed (SF == 1)
                case InstructionType.JS:
                    canJump = isSign; break;
                // Jump if No Signed (SF == 0)
                case InstructionType.JNS:
                    canJump = !isSign; break;
                // Jump if Parity, Jump if Parity is Even (PF == 1)
                case InstructionType.JP:
                    canJump = isParity; break;
                // Jump if Not Parity, Jump if Parity is Odd (PF == 0)
                case InstructionType.JNP:
                    canJump = !isParity; break;

                // Jump if CX Register is zero (CX == 0)
                case InstructionType.JCXZ:
                    canJump = cx == 0; break;

                // Jump if Greater, Jump if Not Less or Equal (ZF == 0 && SF == OF)
                case InstructionType.JG:
                    canJump = !isZero && isSign == isOverflow; break;
                // Jump if Greater or Equal, Jump if Not Less (SF == OF)
                case InstructionType.JGE:
                    canJump = isSign == isOverflow; break;
                // Jump if Less, Jump if Not Greater or Equal (SF != OF)
                case InstructionType.JL:
                    canJump = isSign != isOverflow; break;
                // Jump if Less or Equal, Jump if Not Greater (ZF == 1 || SF != OF)
                case InstructionType.JLE:
                    canJump = isZero || isSign != isOverflow; break;

                // Jump if Above, Jump if Not Below or Equal (ZF == 0 && CF == 0)
                case InstructionType.JA:
                    canJump = !isZero && !isCarry; break;
                // Jump if Above or Equal, Jump if Not Below (CF == 0)
                case InstructionType.JAE:
                    canJump = !isCarry; break;
                // Jump if Below, Jump if Not Above or Equal (CF == 1)
                case InstructionType.JB:
                    canJump = isCarry; break;
                // Jump if Below or Equal, Jump if Not Above  (ZF == 1 || CF == 1)
                case InstructionType.JBE:
                    canJump = isZero || isCarry; break;

                // Decrement CX and Loop if CX == 0
                case InstructionType.LOOP:
                    {
                        --cx;
                        canJump = cx == 0;
                    }
                    break;

                // Decrement CX and Loop if CX == 0 && ZF == 1
                case InstructionType.LOOPE:
                    {
                        --cx;
                        canJump = cx == 0 && isZero;
                    }
                    break;

                // Decrement CX and Loop if CX == 0 && ZF == 0
                case InstructionType.LOOPNZ:
                    {
                        --cx;
                        canJump = cx == 0 && !isZero;
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
                    finalDest = new Immediate((byte)sum);
                    isZero = (byte)sum == 0;
                    isSign = (sbyte)sum < 0;
                    isParity = IsParity((byte)sum);
                    isOverflow = IsOverflow8(sum);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)sum);
                    isZero = (ushort)sum == 0;
                    isSign = (short)sum < 0;
                    isParity = IsParity((ushort)sum);
                    isOverflow = IsOverflow16(sum);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);

            }

            OneOf<byte, Error> storeRes = StoreOperand(cpu, instruction, state, destOperand, finalDest);
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
                    finalDest = new Immediate((byte)sub);
                    isZero = (byte)sub == 0;
                    isSign = (sbyte)sub < 0;
                    isParity = IsParity((byte)sub);
                    isOverflow = IsOverflow8(sub);
                    break;

                case DataWidthType.Word:
                    finalDest = new Immediate((ushort)sub);
                    isZero = (ushort)sub == 0;
                    isSign = (short)sub < 0;
                    isParity = IsParity((ushort)sub);
                    isOverflow = IsOverflow16(sub);
                    break;

                default:
                    return new Error(ErrorCode.MismatchInstructionOperands, $"Unsupported data width type '{instruction.Width.Type}' for {instruction.Mnemonic} instruction", instruction.Position);
            }

            OneOf<byte, Error> storeRes = StoreOperand(cpu, instruction, state, destOperand, finalDest);
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
