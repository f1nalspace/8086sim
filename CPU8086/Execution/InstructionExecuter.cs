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

            _typeFunctionTable[(int)InstructionType.JE] = Jump;
            _typeFunctionTable[(int)InstructionType.JNE] = Jump;
            _typeFunctionTable[(int)InstructionType.JC] = Jump;
            _typeFunctionTable[(int)InstructionType.JNC] = Jump;
            _typeFunctionTable[(int)InstructionType.JO] = Jump;
            _typeFunctionTable[(int)InstructionType.JNO] = Jump;
            _typeFunctionTable[(int)InstructionType.JS] = Jump;
            _typeFunctionTable[(int)InstructionType.JNS] = Jump;
            _typeFunctionTable[(int)InstructionType.JP] = Jump;
            _typeFunctionTable[(int)InstructionType.JNP] = Jump;
            _typeFunctionTable[(int)InstructionType.JCXZ] = Jump;
            _typeFunctionTable[(int)InstructionType.JG] = Jump;
            _typeFunctionTable[(int)InstructionType.JGE] = Jump;
            _typeFunctionTable[(int)InstructionType.JL] = Jump;
            _typeFunctionTable[(int)InstructionType.JLE] = Jump;
            _typeFunctionTable[(int)InstructionType.JA] = Jump;
            _typeFunctionTable[(int)InstructionType.JAE] = Jump;
            _typeFunctionTable[(int)InstructionType.JB] = Jump;
            _typeFunctionTable[(int)InstructionType.JBE] = Jump;
            _typeFunctionTable[(int)InstructionType.JMP] = Jump;
            _typeFunctionTable[(int)InstructionType.LOOP] = Jump;
            _typeFunctionTable[(int)InstructionType.LOOPE] = Jump;
            _typeFunctionTable[(int)InstructionType.LOOPNZ] = Jump;
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

        private static OneOf<int, Error> Jump(CPU cpu, Instruction instruction, IRunState state)
        {
            Contract.Assert(instruction != null);

            if (instruction.Operands.Length != 1)
                return new Error(ErrorCode.MismatchInstructionOperands, $"Invalid number of operands for {instruction.Mnemonic} instruction", instruction.Position);

            InstructionType type = instruction.Mnemonic.Type;

            InstructionOperand labelOperand = instruction.Operands[0];

            var isZero = cpu.Register.ZeroFlag;
            var isCarry = cpu.Register.CarryFlag;
            var isOverflow = cpu.Register.OverflowFlag;
            var isSign = cpu.Register.SignFlag;
            var isParity = cpu.Register.ParityFlag;

            OneOf<Immediate, Error> cxLoad = cpu.LoadRegister(RegisterType.CX);
            if (cxLoad.IsT1)
                return cxLoad.AsT1;
            short cx = cxLoad.AsT0.S16;

            bool updatedCX = false;
            bool canJump = false;
            switch (type)
            {
                case InstructionType.JMP:
                    canJump = true; break;

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
                        updatedCX = true;
                        canJump = cx == 0;
                    }
                    break;

                // Decrement CX and Loop if CX == 0 && ZF == 1
                case InstructionType.LOOPE:
                    {
                        --cx;
                        updatedCX = true;
                        canJump = cx == 0 && isZero;
                    }
                    break;

                // Decrement CX and Loop if CX == 0 && ZF == 0
                case InstructionType.LOOPNZ:
                    {
                        --cx;
                        updatedCX = true;
                        canJump = cx == 0 && !isZero;
                    }
                    break;

                default:
                    return new Error(ErrorCode.UnsupportedInstruction, $"The jump instruction '{type}' is not supported!", instruction.Position);
            }

            if (updatedCX)
            {
                OneOf<byte, Error> storeRes = cpu.StoreRegister(instruction, state, RegisterType.CX, new Immediate(cx, ImmediateFlag.None));
                if (storeRes.IsT1)
                    return storeRes.AsT1;
            }

            if (canJump)
            {
                // Resolve operand
                int relativeAddress;
                switch (labelOperand.Op)
                {
                    case OperandType.Register:
                        {
                            OneOf<Immediate, Error> regLoad = cpu.LoadRegister(labelOperand.Register);
                            if (regLoad.IsT1)
                                return regLoad.AsT1;
                            relativeAddress = regLoad.AsT0.Value;
                        }
                        break;
                    case OperandType.Address:
                        {
                            DataType dataType = WidthToType(instruction.Width);
                            OneOf<Immediate, Error> memLoad = cpu.LoadMemory(labelOperand.Memory, dataType);
                            if (memLoad.IsT1)
                                return memLoad.AsT1;
                            relativeAddress = memLoad.AsT0.Value;
                        }
                        break;
                    case OperandType.Immediate:
                        relativeAddress = labelOperand.Immediate.Value;
                        break;
                    case OperandType.Value:
                        relativeAddress = labelOperand.Value;
                        break;
                    default:
                        return new Error(ErrorCode.UnsupportedOperandType, $"The label operand type '{labelOperand.Op}' is not supported!", instruction.Position);
                }

                return relativeAddress;
            }

            return 0;
        }

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
