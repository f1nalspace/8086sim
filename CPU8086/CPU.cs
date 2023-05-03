using Final.CPU8086.Execution;
using Final.CPU8086.Extensions;
using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using OneOf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Final.CPU8086
{
    /// <summary>
    /// <para>Simulates a 8086 CPU with support for decoding instructions, as well as executing them.</para>
    /// <para>It is *not* cycle exact, but to simulate performance some delays are built-in.</para>
    /// </summary>
    public class CPU : INotifyPropertyChanged
    {
        // A 8086 can execute rougly 4770 instructions per second, resulting in ~0.2 Milliseconds per cycle
        public const double Hz = 4.77 * 1000.0;
        public const double MillisecondPerCycle = 1000.0 / Hz;
        public const double InstructionDecodeMilliseconds = MillisecondPerCycle * 10;

        //
        // Default segmented memory layout
        //
        // [128 KB][64 KB  Data][64 KB  Code][64 KB][64 KB Stack][64 KB][64 KB Extra][512 KB]

        // Unknown: 128 KB 0x00000 to 0x1FFFF

        // Data Segment: 64 KB 0x20000 to 0x2FFFF
        public const uint DataSegmentStart = 0x20000;
        public const uint DataSegmentEnd = 0x2FFFF;
        public const uint DataSegmentLength = DataSegmentEnd - DataSegmentStart;

        // Code Segment: 64 KB 0x30000 to 0x3FFFF
        public const uint CodeSegmentStart = 0x30000;
        public const uint CodeSegmentEnd = 0x3FFFF;
        public const uint CodeSegmentLength = CodeSegmentEnd - CodeSegmentStart;

        // Unknown: 64 KB 0x40000 to 0x4FFFF

        // Stack Segment: 64 KB 0x50000 to 0x5FFFF
        public const uint StackSegmentStart = 0x50000;
        public const uint StackSegmentEnd = 0x5FFFF;
        public const uint StackSegmentLength = StackSegmentEnd - StackSegmentStart;

        // Unknown: 64 KB 0x60000 to 0x6FFFF

        // Extra Segment: 64 KB 0x70000 to 0x7FFFF
        public const uint ExtraSegmentStart = 0x70000;
        public const uint ExtraSegmentEnd = 0x7FFFF;
        public const uint ExtraSegmentLength = ExtraSegmentEnd - ExtraSegmentStart;

        // Unknown: 512 KB 0x80000 to 0xFFFFF

        // The 8086 had 1 MB of total memory available, divided up into several 64 KB segments
        public const uint HighestMemoryAddress = 0xFFFFF;

        private const uint MaxInstructionLength = 6;

        private static readonly REGMappingTable _regTable = new REGMappingTable();
        private static readonly EffectiveAddressCalculationTable _effectiveAddressCalculationTable = new EffectiveAddressCalculationTable();

        private readonly InstructionTable _entryTable = new InstructionTable();

        private readonly InstructionExecuter _executer;

        public event PropertyChangedEventHandler PropertyChanged;

        public event MemoryChangedEventHandler MemoryChanged;

        private static readonly DataWidth FarPointerDataWidth = new DataWidth(DataWidthType.Word);
        private static readonly DataWidth NearPointerDataWidth = new DataWidth(DataWidthType.Byte);

        private static readonly DataType FarPointerDataType = DataType.Word;
        private static readonly DataType NearPointerDataType = DataType.Byte;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetValue<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(target, value))
            {
                target = value;
                RaisePropertyChanged(propertyName);
            }
        }

        public MemoryState Memory { get; }

        public RegisterState Register { get; }

        public IProgram ActiveProgram { get => _activeProgram; private set => SetValue(ref _activeProgram, value); }
        private IProgram _activeProgram = null;

        public ExecutionState ExecutionState { get => _executionState; private set => SetValue(ref _executionState, value); }
        private ExecutionState _executionState = ExecutionState.Stopped;

        public Instruction CurrentInstruction { get => _currentInstruction; private set => SetValue(ref _currentInstruction, value); }
        private Instruction _currentInstruction = null;

        public uint PreviousIP { get => _previousIP; private set => SetValue(ref _previousIP, value); }
        private uint _previousIP = uint.MaxValue;

        public uint CurrentIP { get => _currentIP; private set => SetValue(ref _currentIP, value); }
        private uint _currentIP = uint.MaxValue;

        public CPU()
        {
            _entryTable.Load();

            _executer = new InstructionExecuter(this);

            Memory = new MemoryState();
            Register = new RegisterState();
        }

        public void Reset()
        {
            if (ActiveProgram != null)
                LoadProgram(ActiveProgram);
            else
            {
                Register.Reset();
                RaisePropertyChanged(nameof(Register));

                Memory.Clear();
                RaisePropertyChanged(nameof(Memory));
            }

            ExecutionState = ExecutionState.Stopped;
        }

        public OneOf<int, Error> LoadProgram(IProgram program)
        {
            if (program == null)
                return new Error(ErrorCode.MissingProgramParameter, $"Missing program argument!", 0);
            if (program.Length > CodeSegmentLength)
                return new Error(ErrorCode.ProgramTooLarge, $"The program '{program.Name}' is too large with '{program.Length}' bytes and does not fit in the code segment of max length '{CodeSegmentLength}' bytes!", 0);

            ActiveProgram = program;

            if (program.Register != null)
                Register.Assign(program.Register);
            else
                Register.Reset();
            RaisePropertyChanged(nameof(Register));

            Memory.Clear();
            Memory.Set(CodeSegmentStart, program.Stream.AsSpan());
            RaisePropertyChanged(nameof(Memory));

            return program.Length;
        }

        public uint ComputeCycles(Instruction instruction)
        {
            byte ea = 0;
            foreach (var op in instruction.Operands)
            {
                if (op.Type == OperandType.Address)
                {
                    ea = _effectiveAddressCalculationTable.GetCycles(op.Memory.EAC);
                    break;
                }
            }

            switch (instruction.Type)
            {
                case InstructionType.AAA:
                    break;
                case InstructionType.AAD:
                    break;
                case InstructionType.AAM:
                    break;
                case InstructionType.AAS:
                    break;
                case InstructionType.ADC:
                    break;
                case InstructionType.ADD:
                    break;
                case InstructionType.AND:
                    break;
                case InstructionType.CALL:
                    break;
                case InstructionType.CBW:
                    break;
                case InstructionType.CLC:
                    break;
                case InstructionType.CLD:
                    break;
                case InstructionType.CLI:
                    break;
                case InstructionType.CMC:
                    break;
                case InstructionType.CMP:
                    break;
                case InstructionType.CMPSB:
                    break;
                case InstructionType.CMPSW:
                    break;
                case InstructionType.CWD:
                    break;
                case InstructionType.DAA:
                    break;
                case InstructionType.DAS:
                    break;
                case InstructionType.DEC:
                    break;
                case InstructionType.DIV:
                    break;
                case InstructionType.HLT:
                    break;
                case InstructionType.IDIV:
                    break;
                case InstructionType.IMUL:
                    break;
                case InstructionType.IN:
                    break;
                case InstructionType.INC:
                    break;
                case InstructionType.INT:
                    break;
                case InstructionType.INTO:
                    break;
                case InstructionType.IRET:
                    break;
                case InstructionType.JA:
                    break;
                case InstructionType.JAE:
                    break;
                case InstructionType.JB:
                    break;
                case InstructionType.JBE:
                    break;
                case InstructionType.JC:
                    break;
                case InstructionType.JCXZ:
                    break;
                case InstructionType.JE:
                    break;
                case InstructionType.JG:
                    break;
                case InstructionType.JGE:
                    break;
                case InstructionType.JL:
                    break;
                case InstructionType.JLE:
                    break;
                case InstructionType.JMP:
                    break;
                case InstructionType.JNC:
                    break;
                case InstructionType.JNE:
                    break;
                case InstructionType.JNO:
                    break;
                case InstructionType.JNS:
                    break;
                case InstructionType.JNP:
                    break;
                case InstructionType.JO:
                    break;
                case InstructionType.JP:
                    break;
                case InstructionType.JS:
                    break;
                case InstructionType.LAHF:
                    break;
                case InstructionType.LDS:
                    break;
                case InstructionType.LEA:
                    break;
                case InstructionType.LES:
                    break;
                case InstructionType.LODSB:
                    break;
                case InstructionType.LODSW:
                    break;
                case InstructionType.LOOP:
                    break;
                case InstructionType.LOOPE:
                    break;
                case InstructionType.LOOPNZ:
                    break;
                case InstructionType.MOV:
                    {
                        // https://www.oocities.org/mc_introtocomputers/Instruction_Timing.PDF
                        if (instruction.Operands.Length == 2)
                        {
                            InstructionOperand destOperand = instruction.Operands[0];
                            InstructionOperand sourceOperand = instruction.Operands[1];
                            if (destOperand.Type == OperandType.Address && sourceOperand.Type == OperandType.Register && sourceOperand.Register == RegisterType.AX)
                                return 10U; // ACC -> MEM
                            else if (destOperand.Type == OperandType.Register && destOperand.Register == RegisterType.AX && sourceOperand.Type == OperandType.Address)
                                return 10U; // MEM -> ACC
                            else if (destOperand.Type == OperandType.Register && sourceOperand.Type == OperandType.Register)
                                return 2U; // REG -> REG
                            else if (destOperand.Type == OperandType.Register && sourceOperand.Type == OperandType.Address)
                                return 8U + ea; // MEM -> REG + EA
                            else if (destOperand.Type == OperandType.Address && sourceOperand.Type == OperandType.Register)
                                return 9U + ea; // REG -> MEM + EA
                            else if (destOperand.Type == OperandType.Register && sourceOperand.Type == OperandType.Immediate)
                                return 4U; // IMM -> REG
                            else if (destOperand.Type == OperandType.Address && sourceOperand.Type == OperandType.Immediate)
                                return 10U + ea; // IMM -> MEM + EA
                            else if (destOperand.Type == OperandType.Register && (destOperand.Register == RegisterType.SS || destOperand.Register == RegisterType.DS || destOperand.Register == RegisterType.ES) && sourceOperand.Type == OperandType.Register)
                                return 2U; // REG -> SS, DS, ES
                            else if (destOperand.Type == OperandType.Register && (destOperand.Register == RegisterType.SS || destOperand.Register == RegisterType.DS || destOperand.Register == RegisterType.ES) && sourceOperand.Type == OperandType.Address)
                                return 8U + ea; // MEM -> SS, DS, ES
                        }
                    }
                    break;
                case InstructionType.MOVSB:
                    break;
                case InstructionType.MOVSW:
                    break;
                case InstructionType.MUL:
                    break;
                case InstructionType.NEG:
                    break;
                case InstructionType.NOP:
                    break;
                case InstructionType.NOT:
                    break;
                case InstructionType.OR:
                    break;
                case InstructionType.OUT:
                    break;
                case InstructionType.POP:
                    break;
                case InstructionType.POPF:
                    break;
                case InstructionType.PUSH:
                    break;
                case InstructionType.PUSHF:
                    break;
                case InstructionType.RCL:
                    break;
                case InstructionType.RCR:
                    break;
                case InstructionType.REPE:
                    break;
                case InstructionType.RET:
                    break;
                case InstructionType.RETF:
                    break;
                case InstructionType.ROL:
                    break;
                case InstructionType.ROR:
                    break;
                case InstructionType.SAHF:
                    break;
                case InstructionType.SAL:
                    break;
                case InstructionType.SHL:
                    break;
                case InstructionType.SAR:
                    break;
                case InstructionType.SBB:
                    break;
                case InstructionType.SCASB:
                    break;
                case InstructionType.SCASW:
                    break;
                case InstructionType.SHR:
                    break;
                case InstructionType.STC:
                    break;
                case InstructionType.STD:
                    break;
                case InstructionType.STI:
                    break;
                case InstructionType.STOSB:
                    break;
                case InstructionType.STOSW:
                    break;
                case InstructionType.SUB:
                    break;
                case InstructionType.TEST:
                    break;
                case InstructionType.WAIT:
                    break;
                case InstructionType.XCHG:
                    break;
                case InstructionType.XLAT:
                    break;
                case InstructionType.XOR:
                    break;
                case InstructionType.LOCK:
                    break;
                case InstructionType.REPNE:
                    break;
                case InstructionType.REP:
                    break;
                case InstructionType.CS:
                    break;
                case InstructionType.SS:
                    break;
                case InstructionType.DS:
                    break;
                case InstructionType.ES:
                    break;
                case InstructionType.FS:
                    break;
                case InstructionType.GS:
                    break;
                case InstructionType.DATA8:
                    break;
                case InstructionType.DATA16:
                    break;
                case InstructionType.ADDR8:
                    break;
                case InstructionType.ADDR16:
                    break;
                default:
                    break;
            }
            return 0;
        }

        public OneOf<Immediate, Error> LoadRegister(RegisterType type)
        {
            return type switch
            {
                RegisterType.AX => new Immediate(Register.AX),
                RegisterType.AL => new Immediate(Register.AL),
                RegisterType.AH => new Immediate(Register.AH),

                RegisterType.BX => new Immediate(Register.BX),
                RegisterType.BL => new Immediate(Register.BL),
                RegisterType.BH => new Immediate(Register.BH),

                RegisterType.CX => new Immediate(Register.CX),
                RegisterType.CL => new Immediate(Register.CL),
                RegisterType.CH => new Immediate(Register.CH),

                RegisterType.DX => new Immediate(Register.DX),
                RegisterType.DL => new Immediate(Register.DL),
                RegisterType.DH => new Immediate(Register.DH),

                RegisterType.SP => new Immediate(Register.SP),
                RegisterType.BP => new Immediate(Register.BP),
                RegisterType.SI => new Immediate(Register.SI),
                RegisterType.DI => new Immediate(Register.DI),

                RegisterType.CS => new Immediate(Register.CS),
                RegisterType.DS => new Immediate(Register.DS),
                RegisterType.SS => new Immediate(Register.SS),
                RegisterType.ES => new Immediate(Register.ES),

                _ => new Error(ErrorCode.UnsupportedRegisterType, $"The register type '{type}' is not supported", 0),
            };
        }

        public static int GetDataTypeSize(DataType type)
        {
            // NOTE(final): We either have a pointer or a type with a pointer, so we strip out the pointer and we are left with the actual type
            if (type == DataType.Pointer)
                type = FarPointerDataType;
            else if (type.HasFlag(DataType.Pointer))
                type ^= ~DataType.Pointer;
            return type switch
            {
                DataType.Byte => 1,
                DataType.Word => 2,
                DataType.DoubleWord or
                DataType.Int => 4,
                DataType.QuadWord => 8,
                _ => 0,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short ImmediateToS16(Immediate value)
        {
            return value.Type switch
            {
                ImmediateType.Byte => (short)value.U8,
                ImmediateType.SignedByte => (short)value.S8,
                ImmediateType.Word => (short)value.U16,
                ImmediateType.SignedWord => value.S16,
                _ => 0,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static sbyte ImmediateToS8(Immediate value)
        {
            return value.Type switch
            {
                ImmediateType.Byte => (sbyte)value.U8,
                ImmediateType.SignedByte => value.S8,
                ImmediateType.Word => (sbyte)value.U16,
                ImmediateType.SignedWord => (sbyte)value.S16,
                _ => 0,
            };
        }

        public OneOf<byte, Error> StoreRegister(Instruction instruction, IRunState state, RegisterType type, Immediate value)
        {
            Immediate oldValue;
            byte result;
            switch (type)
            {
                case RegisterType.AX:
                    oldValue = new Immediate(Register.AX);
                    Register.AX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.AL:
                    oldValue = new Immediate(Register.AL);
                    Register.AL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.AH:
                    oldValue = new Immediate(Register.AH);
                    Register.AH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.BX:
                    oldValue = new Immediate(Register.BX);
                    Register.BX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.BL:
                    oldValue = new Immediate(Register.BL);
                    Register.BL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.BH:
                    oldValue = new Immediate(Register.BH);
                    Register.BH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.CX:
                    oldValue = new Immediate(Register.CX);
                    Register.CX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.CL:
                    oldValue = new Immediate(Register.CL);
                    Register.CL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.CH:
                    oldValue = new Immediate(Register.CH);
                    Register.CH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.DX:
                    oldValue = new Immediate(Register.DX);
                    Register.DX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.DL:
                    oldValue = new Immediate(Register.DL);
                    Register.DL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.DH:
                    oldValue = new Immediate(Register.DH);
                    Register.DH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.SP:
                    oldValue = new Immediate(Register.SP);
                    Register.SP = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.BP:
                    oldValue = new Immediate(Register.BP);
                    Register.BP = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.SI:
                    oldValue = new Immediate(Register.SI);
                    Register.SI = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.DI:
                    oldValue = new Immediate(Register.DI);
                    Register.DI = ImmediateToS16(value);
                    result = 2;
                    break;

                case RegisterType.CS:
                    oldValue = new Immediate(Register.CS);
                    Register.CS = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.DS:
                    oldValue = new Immediate(Register.DS);
                    Register.DS = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.SS:
                    oldValue = new Immediate(Register.SS);
                    Register.SS = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.ES:
                    oldValue = new Immediate(Register.ES);
                    Register.ES = ImmediateToS16(value);
                    result = 2;
                    break;

                default:
                    return new Error(ErrorCode.UnsupportedRegisterType, $"The register type '{type}' is not supported", 0);
            }

            state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(type, oldValue), new ExecutedValue(type, value))));

            return result;
        }

        

        static RegisterType SegmentToRegister(SegmentType type)
        {
            return type switch
            {
                SegmentType.CS => RegisterType.CS,
                SegmentType.DS => RegisterType.DS,
                SegmentType.SS => RegisterType.SS,
                SegmentType.ES => RegisterType.ES,
                _ => RegisterType.Unknown,
            };
        }

        public uint GetSegmentAddress(SegmentType type, uint segmentAddress, int offset)
        {
            // Get segment base offset
            uint segmentBase;
            switch (type)
            {
                case SegmentType.Direct:
                    segmentBase = segmentAddress;
                    break;
                case SegmentType.CS:
                case SegmentType.DS:
                case SegmentType.SS:
                case SegmentType.ES:
                    {
                        RegisterType segmentRegister = SegmentToRegister(type);
                        OneOf<Immediate, Error> loadedSegment = LoadRegister(segmentRegister);
                        if (loadedSegment.IsT1)
                            return uint.MaxValue;
                        segmentBase = loadedSegment.AsT0.U16;
                    }
                    break;
                default:
                    segmentBase = 0;
                    break;
            }

            uint result = (uint)((segmentBase << 4) + offset);
            return result;
        }

        private uint GetAbsoluteMemoryAddress(MemoryAddress address)
        {
            byte u8 = (byte)(address.Displacement & 0xFF);
            ushort u16 = (ushort)(address.Displacement & 0xFFFF);
            int d8 = (u8 & 0b10000000) == 0b10000000 ? (-u8) : u8;
            int d16 = (u16 & 0b10000000_00000000) == 0b10000000_00000000 ? (-u16) : u16;

            int offset = address.EAC switch
            {
                EffectiveAddressCalculation.BX_SI => Register.BX + Register.SI,
                EffectiveAddressCalculation.BX_DI => Register.BX + Register.DI,
                EffectiveAddressCalculation.BP_SI => Register.BP + Register.SI,
                EffectiveAddressCalculation.BP_DI => Register.BP + Register.DI,
                EffectiveAddressCalculation.SI => Register.SI,
                EffectiveAddressCalculation.DI => Register.DI,
                EffectiveAddressCalculation.DirectAddress => address.Displacement & 0xFFFF,
                EffectiveAddressCalculation.BX => Register.BX,
                EffectiveAddressCalculation.BX_SI_D8 => Register.BX + Register.SI + d8,
                EffectiveAddressCalculation.BX_DI_D8 => Register.BX + Register.DI + d8,
                EffectiveAddressCalculation.BP_SI_D8 => Register.BP + Register.SI + d8,
                EffectiveAddressCalculation.BP_DI_D8 => Register.BP + Register.DI + d8,
                EffectiveAddressCalculation.SI_D8 => Register.SI + d8,
                EffectiveAddressCalculation.DI_D8 => Register.DI + d8,
                EffectiveAddressCalculation.BP_D8 => Register.BP + d8,
                EffectiveAddressCalculation.BX_D8 => Register.BX + d8,
                EffectiveAddressCalculation.BX_SI_D16 => Register.BX + Register.SI + d16,
                EffectiveAddressCalculation.BX_DI_D16 => Register.BX + Register.DI + d16,
                EffectiveAddressCalculation.BP_SI_D16 => Register.BP + Register.SI + d16,
                EffectiveAddressCalculation.BP_DI_D16 => Register.BP + Register.DI + d16,
                EffectiveAddressCalculation.SI_D16 => Register.SI + d16,
                EffectiveAddressCalculation.DI_D16 => Register.DI + d16,
                EffectiveAddressCalculation.BP_D16 => Register.BP + d16,
                EffectiveAddressCalculation.BX_D16 => Register.BX + d16,
                _ => int.MinValue,
            };

            // Exit out with an large address when we got an unsupported EAC
            if (offset == int.MinValue)
                return uint.MaxValue;

            uint result = GetSegmentAddress(address.SegmentType, address.SegmentAddress, offset);

            return result;
        }

        private static DataType ResolvePointerDataType(DataType type)
        {
            if (!type.HasFlag(DataType.Pointer))
                return DataType.None;
            if (type == DataType.Pointer)
                return FarPointerDataType;
            return type ^ ~DataType.Pointer;
        }

        private OneOf<Immediate, Error> LoadMemory(uint absoluteAddress, DataType type)
        {
            if (type.HasFlag(DataType.Pointer))
            {
                DataType pointerType = ResolvePointerDataType(type);
                if (pointerType == DataType.None)
                    return new Error(ErrorCode.UnsupportedDataType, $"The pointer type '{type}' is not supported", 0);
                type = pointerType;
            }

            int typeSize = GetDataTypeSize(type);
            if (absoluteAddress < 0 || (absoluteAddress + typeSize) >= Memory.Length)
                return new Error(ErrorCode.InvalidMemoryAddress, $"The absolute source memory address '{absoluteAddress}' is not valid for type '{type}'!", 0);

            switch (type)
            {
                case DataType.Byte:
                    return new Immediate(Memory[absoluteAddress]);

                case DataType.Word:
                    {
                        ushort u16 = (ushort)(
                            (Memory[absoluteAddress + 0] << 0) | 
                            (Memory[absoluteAddress + 1] << 8));
                        return new Immediate(u16);
                    }

                case DataType.Int:
                case DataType.DoubleWord:
                    {
                        uint u32 = (uint)(
                            (Memory[absoluteAddress + 0] << 0) |
                            (Memory[absoluteAddress + 1] << 8) |
                            (Memory[absoluteAddress + 2] << 16) |
                            (Memory[absoluteAddress + 3] << 24));
                        if (type == DataType.Int)
                            return new Immediate((int)u32);
                        else
                            return new Immediate(u32);
                    }

                default:
                    return new Error(ErrorCode.UnsupportedDataType, $"The source memory type '{type}' is not supported!", 0);
            }
        }

        public OneOf<Immediate, Error> LoadMemory(MemoryAddress address, DataType type)
        {
            uint absoluteAddress = GetAbsoluteMemoryAddress(address);
            if (absoluteAddress == uint.MinValue)
                return new Error(ErrorCode.UnsupportedEffectiveAddressCalculation, $"The effective address calculation '{address.EAC}' is not supported for the specified memory address '{address}' for type '{type}'", 0);
            return LoadMemory(absoluteAddress, type);
        }

        public OneOf<byte, Error> StoreMemory(Instruction instruction, IRunState state, MemoryAddress address, DataType type, Immediate value)
        {
            uint absoluteAddress = GetAbsoluteMemoryAddress(address);
            if (absoluteAddress == uint.MaxValue)
                return new Error(ErrorCode.UnsupportedEffectiveAddressCalculation, $"The effective address calculation '{address.EAC}' is not supported for the specified memory address '{address}' for type '{type}'", 0);

            if (type.HasFlag(DataType.Pointer))
            {
                DataType pointerType = ResolvePointerDataType(type);
                if (pointerType == DataType.None)
                    return new Error(ErrorCode.UnsupportedDataType, $"The type '{type}' is not supported by a pointer", 0);
                type = pointerType;
            }

            int typeSize = GetDataTypeSize(type);
            if ((absoluteAddress + typeSize) >= Memory.Length)
                return new Error(ErrorCode.InvalidMemoryAddress, $"The absolute destination memory address '{absoluteAddress}' is not valid for type '{type}'!", 0);

            switch (type)
            {
                case DataType.Byte:
                    {
                        Immediate oldValue = new Immediate(Memory[absoluteAddress]);
                        Memory[absoluteAddress] = value.U8;

                        MemoryChanged?.Invoke(this, new MemoryChangedEventArgs(absoluteAddress, 1));

                        state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(address, oldValue), new ExecutedValue(address, value))));
                    }
                    return 1;

                case DataType.Word:
                    {
                        ushort oldValue = (ushort)((Memory[absoluteAddress + 0] << 0) | (Memory[absoluteAddress + 1] << 8));
                        Immediate oldMemory = new Immediate(oldValue);

                        ushort newValue = value.U16;
                        Memory[absoluteAddress + 0] = (byte)((newValue >> 0) & 0xFF);
                        Memory[absoluteAddress + 1] = (byte)((newValue >> 8) & 0xFF);

                        state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(address, oldMemory), new ExecutedValue(address, value))));

                        MemoryChanged?.Invoke(this, new MemoryChangedEventArgs(absoluteAddress, 2));

                        return 2;
                    }

                case DataType.Int:
                case DataType.DoubleWord:
                    {
                        uint oldValue = (uint)(
                            (Memory[absoluteAddress + 0] << 0) | 
                            (Memory[absoluteAddress + 1] << 8) | 
                            (Memory[absoluteAddress + 2] << 16) | 
                            (Memory[absoluteAddress + 3] << 24));
                        Immediate oldMemory;
                        if (type == DataType.Int)
                            oldMemory = new Immediate((int)oldValue);
                        else
                            oldMemory = new Immediate(oldValue);

                        uint newValue = value.U32;
                        Memory[absoluteAddress + 0] = (byte)((newValue >> 0) & 0xFF);
                        Memory[absoluteAddress + 1] = (byte)((newValue >> 8) & 0xFF);
                        Memory[absoluteAddress + 2] = (byte)((newValue >> 16) & 0xFF);
                        Memory[absoluteAddress + 3] = (byte)((newValue >> 24) & 0xFF);

                        state.AddExecuted(new ExecutedInstruction(instruction, new ExecutedChange(new ExecutedValue(address, oldMemory), new ExecutedValue(address, value))));

                        MemoryChanged?.Invoke(this, new MemoryChangedEventArgs(absoluteAddress, 4));

                        return 4;
                    }

                default:
                    return new Error(ErrorCode.UnsupportedDataWidth, $"The destination memory type '{type}' is not supported!", 0);
            }
        }

        private static OneOf<byte, Error> ReadU8(ref ReadOnlySpan<byte> stream, string streamName, uint position)
        {
            if (stream.Length < 1)
                return new Error(ErrorCode.NotEnoughBytesInStream, $"Cannot read U8, because stream '{streamName}' is already finished or is not long enough for 1 byte", position);
            byte result = stream[0];
            stream = stream.Slice(1);
            return result;
        }

        public OneOf<string, Error> GetAssembly(Stream stream, string name, OutputValueMode outputMode)
        {
            long len = stream.Length;
            byte[] data = new byte[len];
            stream.Read(data);
            return GetAssembly(data, name, outputMode);
        }

        public Instruction DecodeNext(ReadOnlySpan<byte> stream, string streamName, uint position = 0)
        {
            OneOf<Instruction, Error> r = TryDecodeNext(stream, streamName, position);
            if (r.IsT0)
                return r.AsT0;
            return null;
        }

        static InstructionOperand CreateOperand(OperandDefinition sourceOp, ModType mode, byte registerBits, EffectiveAddressCalculation eac, int displacement, int immediate, int offset, SegmentType segmentType, uint segmentAddress, DataType type)
        {
            switch (sourceOp.Kind)
            {
                case OperandDefinitionKind.Value:
                    return new InstructionOperand(s32: sourceOp.Value);

                case OperandDefinitionKind.MemoryByte:
                    return new InstructionOperand(new MemoryAddress(eac, displacement, segmentType, segmentAddress));

                case OperandDefinitionKind.MemoryWord:
                    return new InstructionOperand(new MemoryAddress(eac, displacement, segmentType, segmentAddress));

                case OperandDefinitionKind.MemoryDoubleWord:
                    return new InstructionOperand(new MemoryAddress(eac, displacement, segmentType, segmentAddress));

                case OperandDefinitionKind.MemoryQuadWord:
                    return new InstructionOperand(new MemoryAddress(eac, displacement, segmentType, segmentAddress));

                case OperandDefinitionKind.MemoryWordReal:
                case OperandDefinitionKind.MemoryDoubleWordReal:
                case OperandDefinitionKind.MemoryQuadWordReal:
                case OperandDefinitionKind.MemoryTenByteReal:
                    break;

                case OperandDefinitionKind.SourceRegister:
                    {
                        if (type == DataType.Byte)
                            return new InstructionOperand(_regTable.GetByte(registerBits));
                        else if (type == DataType.Word)
                            return new InstructionOperand(_regTable.GetWord(registerBits));
                        else
                            throw new NotSupportedException($"Unsupported type of '{type}' for source register");
                    }

                case OperandDefinitionKind.RegisterByte:
                    return new InstructionOperand(_regTable.GetByte(registerBits));
                case OperandDefinitionKind.RegisterWord:
                    return new InstructionOperand(_regTable.GetWord(registerBits));
                case OperandDefinitionKind.RegisterDoubleWord:
                    break;

                case OperandDefinitionKind.RegisterOrMemoryByte:
                    if (mode == ModType.RegisterMode)
                        return new InstructionOperand(_regTable.GetByte(registerBits));
                    else
                        return new InstructionOperand(new MemoryAddress(eac, displacement, segmentType, segmentAddress));
                case OperandDefinitionKind.RegisterOrMemoryWord:
                    if (mode == ModType.RegisterMode)
                        return new InstructionOperand(_regTable.GetWord(registerBits));
                    else
                        return new InstructionOperand(new MemoryAddress(eac, displacement, segmentType, segmentAddress));
                case OperandDefinitionKind.RegisterOrMemoryDoubleWord:
                case OperandDefinitionKind.RegisterOrMemoryQuadWord:
                    break;

                case OperandDefinitionKind.ImmediateByte:
                    if ((sbyte)immediate < 0)
                        return new InstructionOperand((sbyte)(immediate & 0xFF), ImmediateFlag.None);
                    else
                        return new InstructionOperand((byte)(immediate & 0xFF), ImmediateFlag.None);
                case OperandDefinitionKind.ImmediateWord:
                    if ((short)immediate < 0)
                        return new InstructionOperand((short)(immediate & 0xFFFF), ImmediateFlag.None);
                    else
                        return new InstructionOperand((ushort)(immediate & 0xFFFF), ImmediateFlag.None);
                case OperandDefinitionKind.ImmediateDoubleWord:
                    if ((int)immediate < 0)
                        return new InstructionOperand((int)(immediate & 0xFFFFFFFF), ImmediateFlag.None);
                    else
                        return new InstructionOperand((uint)(immediate & 0xFFFFFFFF), ImmediateFlag.None);

                case OperandDefinitionKind.TypeDoubleWord:
                    break;
                case OperandDefinitionKind.TypeShort:
                    break;
                case OperandDefinitionKind.TypeInt:
                    break;
                case OperandDefinitionKind.TypePointer:
                    break;

                case OperandDefinitionKind.ShortLabel:
                    if ((sbyte)displacement < 0)
                        return new InstructionOperand((sbyte)(displacement & 0xFF), ImmediateFlag.RelativeJumpDisplacement);
                    else
                        return new InstructionOperand((byte)(displacement & 0xFF), ImmediateFlag.RelativeJumpDisplacement);
                case OperandDefinitionKind.LongLabel:
                    if ((short)displacement < 0)
                        return new InstructionOperand((short)(displacement & 0xFFFF), ImmediateFlag.RelativeJumpDisplacement);
                    else
                        return new InstructionOperand((ushort)(displacement & 0xFFFF), ImmediateFlag.RelativeJumpDisplacement);

                case OperandDefinitionKind.FarPointer:
                    return new InstructionOperand(new MemoryAddress(EffectiveAddressCalculation.DirectAddress, (short)(offset & 0xFFFF), segmentType, segmentAddress));

                case OperandDefinitionKind.NearPointer:
                    return new InstructionOperand(new MemoryAddress(EffectiveAddressCalculation.DirectAddress, (sbyte)(offset & 0xFF), segmentType, segmentAddress));

                case OperandDefinitionKind.ST:
                    break;
                case OperandDefinitionKind.ST_I:
                    break;
                case OperandDefinitionKind.M:
                    break;
                case OperandDefinitionKind.M_Number:
                    break;

                case OperandDefinitionKind.RAX:
                    return new InstructionOperand(RegisterType.RAX);
                case OperandDefinitionKind.EAX:
                    return new InstructionOperand(RegisterType.EAX);
                case OperandDefinitionKind.AX:
                    return new InstructionOperand(RegisterType.AX);
                case OperandDefinitionKind.AL:
                    return new InstructionOperand(RegisterType.AL);
                case OperandDefinitionKind.AH:
                    return new InstructionOperand(RegisterType.AH);

                case OperandDefinitionKind.RBX:
                    return new InstructionOperand(RegisterType.RBX);
                case OperandDefinitionKind.EBX:
                    return new InstructionOperand(RegisterType.EBX);
                case OperandDefinitionKind.BX:
                    return new InstructionOperand(RegisterType.BX);
                case OperandDefinitionKind.BL:
                    return new InstructionOperand(RegisterType.BL);
                case OperandDefinitionKind.BH:
                    return new InstructionOperand(RegisterType.BH);

                case OperandDefinitionKind.RCX:
                    return new InstructionOperand(RegisterType.RCX);
                case OperandDefinitionKind.ECX:
                    return new InstructionOperand(RegisterType.ECX);
                case OperandDefinitionKind.CX:
                    return new InstructionOperand(RegisterType.CX);
                case OperandDefinitionKind.CL:
                    return new InstructionOperand(RegisterType.CL);
                case OperandDefinitionKind.CH:
                    return new InstructionOperand(RegisterType.CH);

                case OperandDefinitionKind.RDX:
                    return new InstructionOperand(RegisterType.RDX);
                case OperandDefinitionKind.EDX:
                    return new InstructionOperand(RegisterType.EDX);
                case OperandDefinitionKind.DX:
                    return new InstructionOperand(RegisterType.DX);
                case OperandDefinitionKind.DL:
                    return new InstructionOperand(RegisterType.DL);
                case OperandDefinitionKind.DH:
                    return new InstructionOperand(RegisterType.DH);

                case OperandDefinitionKind.RSP:
                    return new InstructionOperand(RegisterType.RSP);
                case OperandDefinitionKind.ESP:
                    return new InstructionOperand(RegisterType.ESP);
                case OperandDefinitionKind.SP:
                    return new InstructionOperand(RegisterType.SP);

                case OperandDefinitionKind.RBP:
                    return new InstructionOperand(RegisterType.RBP);
                case OperandDefinitionKind.EBP:
                    return new InstructionOperand(RegisterType.EBP);
                case OperandDefinitionKind.BP:
                    return new InstructionOperand(RegisterType.BP);

                case OperandDefinitionKind.RSI:
                    return new InstructionOperand(RegisterType.RSI);
                case OperandDefinitionKind.ESI:
                    return new InstructionOperand(RegisterType.ESI);
                case OperandDefinitionKind.SI:
                    return new InstructionOperand(RegisterType.SI);

                case OperandDefinitionKind.RDI:
                    return new InstructionOperand(RegisterType.RDI);
                case OperandDefinitionKind.EDI:
                    return new InstructionOperand(RegisterType.EDI);
                case OperandDefinitionKind.DI:
                    return new InstructionOperand(RegisterType.DI);

                case OperandDefinitionKind.CS:
                    return new InstructionOperand(RegisterType.CS);
                case OperandDefinitionKind.DS:
                    return new InstructionOperand(RegisterType.DS);
                case OperandDefinitionKind.SS:
                    return new InstructionOperand(RegisterType.SS);
                case OperandDefinitionKind.ES:
                    return new InstructionOperand(RegisterType.ES);

                case OperandDefinitionKind.CR:
                    return new InstructionOperand(RegisterType.CR);
                case OperandDefinitionKind.DR:
                    return new InstructionOperand(RegisterType.DR);
                case OperandDefinitionKind.TR:
                    return new InstructionOperand(RegisterType.TR);
                case OperandDefinitionKind.FS:
                    return new InstructionOperand(RegisterType.FS);
                case OperandDefinitionKind.GS:
                    return new InstructionOperand(RegisterType.GS);

                default:
                    break;
            }

            throw new NotSupportedException($"The source operand '{sourceOp}' is not supported");
        }

        static DataWidth DataTypeToDataWidth(DataType type)
        {
            return type switch
            {
                DataType.Byte => new DataWidth(DataWidthType.Byte),
                DataType.Word => new DataWidth(DataWidthType.Word),
                DataType.DoubleWord => new DataWidth(DataWidthType.DoubleWord),
                DataType.Int => new DataWidth(DataWidthType.DoubleWord),
                DataType.QuadWord => new DataWidth(DataWidthType.QuadWord),
                DataType.Pointer => FarPointerDataWidth,
                _ => new DataWidth()
            };
        }

        static DataType DataWidthToDataType(DataWidth width)
        {
            return width.Type switch
            {
                DataWidthType.Byte => DataType.Byte,
                DataWidthType.Word => DataType.Word,
                DataWidthType.DoubleWord => DataType.DoubleWord,
                DataWidthType.QuadWord => DataType.QuadWord,
                _ => DataType.None
            };
        }

        static OneOf<Instruction, Error> DecodeInstruction(ReadOnlySpan<byte> stream, string streamName, uint position, InstructionEntry entry)
        {
            if (stream.Length == 0)
                return new Error(ErrorCode.EndOfStream, $"Expect at least one byte of stream length!", position);

            byte opCode = stream[0];
            if (opCode != entry.Op)
                return new Error(ErrorCode.OpCodeMismatch, $"Expect op-code '{entry.Op}', but got '{opCode}'", position);

#if DEBUG
            StringBuilder streamBytes = new StringBuilder();
            int remainingBytes = 0;
            if (stream.Length >= entry.MaxLength)
                remainingBytes = entry.MaxLength;
            else if (stream.Length >= entry.MinLength)
                remainingBytes = Math.Min(stream.Length, entry.MaxLength);
            else
                remainingBytes = stream.Length;
            for (int i = 0; i < remainingBytes; i++)
            {
                if (streamBytes.Length > 0)
                    streamBytes.Append(' ');
                streamBytes.Append(stream[i].ToString("X2"));
            }
            if (stream.Length > remainingBytes)
                streamBytes.Append(" ...");
            Debug.WriteLine($"Load instruction '{entry}' with bytes ({streamBytes})");
#endif

            InstructionFlags flags = entry.Flags;

            ReadOnlySpan<byte> cur = stream.Slice(1); // Skip op-code

            bool destinationIsRegister = (opCode & 0b00000010) == 0b00000010;

            byte modField = byte.MaxValue;
            byte regField = byte.MaxValue;
            byte rmField = byte.MaxValue;

            int displacement = 0;
            int immediate = 0;
            int offset = 0;
            uint segmentAddress = 0;
            SegmentType segmentType = SegmentType.None;

            ModType mode = ModType.Unknown;

            EffectiveAddressCalculation eac = EffectiveAddressCalculation.None;

            int displacementLength = 0;

            bool hasRegField = false;

            foreach (FieldDefinition field in entry.Fields)
            {
                switch (field.Type)
                {
                    case FieldDefinitionType.Constant:
                        {
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return value.AsT1;
                            if (field.Value != value.AsT0)
                                return new Error(ErrorCode.ConstantFieldMismatch, $"Expect constant to be '{field.Value}', but got instead '{value.AsT0}' in field '{field}'", position);
                        }
                        break;
                    case FieldDefinitionType.ModRegRM:
                    case FieldDefinitionType.Mod000RM:
                    case FieldDefinitionType.Mod001RM:
                    case FieldDefinitionType.Mod010RM:
                    case FieldDefinitionType.Mod011RM:
                    case FieldDefinitionType.Mod100RM:
                    case FieldDefinitionType.Mod101RM:
                    case FieldDefinitionType.Mod110RM:
                    case FieldDefinitionType.Mod111RM:
                        {
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return value.AsT1;
                            byte mrm = value.AsT0;
                            modField = (byte)(mrm >> 6 & 0b111);
                            regField = (byte)(mrm >> 3 & 0b111);
                            rmField = (byte)(mrm >> 0 & 0b111);
                            if (field.Type != FieldDefinitionType.ModRegRM)
                            {
                                hasRegField = false;
                                byte expectReg = field.Type - FieldDefinitionType.Mod000RM;
                                if (expectReg != regField)
                                    return new Error(ErrorCode.ConstantFieldMismatch, $"Expect register constant to be '{expectReg}', but got '{regField}' instead in field '{field}'", position);
                            }
                            else
                                hasRegField = true;
                            mode = (ModType)modField;
                            eac = mode switch
                            {
                                ModType.RegisterMode => EffectiveAddressCalculation.None,
                                _ => _effectiveAddressCalculationTable.Get(rmField, modField)
                            };
                            if (mode != ModType.RegisterMode)
                                displacementLength = _effectiveAddressCalculationTable.GetDisplacementLength(eac);
                            else
                                displacementLength = 0;
                        }
                        break;
                    case FieldDefinitionType.Displacement0:
                    case FieldDefinitionType.Displacement1:
                        {
                            int t = field.Type - FieldDefinitionType.Displacement0;
                            if (modField == byte.MaxValue || (displacementLength > 0 && t < displacementLength))
                            {
                                OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                                if (value.IsT1)
                                    return new Error(value.AsT1, $"No more bytes left for reading the displacement-{t} in field '{field}'", position);
                                byte d8 = value.AsT0;
                                int shift = t * 8;
                                displacement |= ((int)d8 << shift);
                            }
                        }
                        break;
                    case FieldDefinitionType.Immediate0:
                    case FieldDefinitionType.Immediate1:
                    case FieldDefinitionType.Immediate2:
                    case FieldDefinitionType.Immediate3:
                        {
                            int t = (int)field.Type - (int)FieldDefinitionType.Immediate0;
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the immediate-{t} in field '{field}'", position);
                            byte imm8 = value.AsT0;
                            int shift = t * 8;
                            immediate |= ((int)imm8 << shift);
                        }
                        break;

                    case FieldDefinitionType.RelativeLabelDisplacement0:
                    case FieldDefinitionType.RelativeLabelDisplacement1:
                        {
                            int t = field.Type - FieldDefinitionType.RelativeLabelDisplacement0;
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the relative label displacement-{t} in field '{field}'", position);
                            byte d8 = value.AsT0;
                            int shift = t * 8;
                            displacement |= ((int)d8 << shift);
                        }
                        break;

                    case FieldDefinitionType.Immediate0to3:
                        {
                            for (int t = 0; t < 4; t++)
                            {
                                OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                                if (value.IsT1)
                                    return new Error(value.AsT1, $"No more bytes left for reading the immediate-{t} in field '{field}'", position);
                                byte imm8 = value.AsT0;
                                int shift = t * 8;
                                immediate |= ((int)imm8 << shift);
                            }
                        }
                        break;

                    case FieldDefinitionType.Offset0:
                    case FieldDefinitionType.Offset1:
                        {
                            int t = (int)(field.Type - FieldDefinitionType.Offset0);
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the offset-{t} in field '{field}'", position);
                            byte offset8 = value.AsT0;
                            int shift = t * 8;
                            offset |= ((int)offset8 << shift);
                        }
                        break;

                    case FieldDefinitionType.Segment0:
                    case FieldDefinitionType.Segment1:
                        {
                            int t = (int)(field.Type - FieldDefinitionType.Segment0);
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the segment-{t} in field '{field}'", position);
                            byte seg8 = value.AsT0;
                            int shift = t * 8;
                            segmentAddress |= ((uint)seg8 << shift);
                            segmentType = SegmentType.Direct;
                        }
                        break;

                    case FieldDefinitionType.ShortLabelOrShortLow:
                        {
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the low displacement in field '{field}'", position);
                            byte low = value.AsT0;
                            displacement |= ((int)low << 0);
                        }
                        break;

                    case FieldDefinitionType.LongLabel:
                    case FieldDefinitionType.ShortHigh:
                        {
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the low displacement in field '{field}'", position);
                            byte high = value.AsT0;
                            displacement |= ((int)high << 8);
                        }
                        break;

                    default:
                        return new Error(ErrorCode.UnsupportedFieldType, $"The field type '{field.Type}' is not supported!", position);
                }
            }

            Span<InstructionOperand> targetOps = stackalloc InstructionOperand[4];
            int opCount = 0;

            if (modField == byte.MaxValue)
                eac = EffectiveAddressCalculation.DirectAddress;

            DataType bestInferredType = DataType.None;
            if (entry.DataWidth.Type == DataWidthType.None)
            {
                foreach (OperandDefinition sourceOp in entry.Operands)
                {
                    DataType inferredType = sourceOp.Kind switch
                    {
                        OperandDefinitionKind.MemoryByte => DataType.Byte,
                        OperandDefinitionKind.MemoryWord => DataType.Word,
                        OperandDefinitionKind.MemoryDoubleWord => DataType.DoubleWord,
                        OperandDefinitionKind.MemoryQuadWord => DataType.QuadWord,

                        OperandDefinitionKind.MemoryWordReal => DataType.Word,
                        OperandDefinitionKind.MemoryDoubleWordReal => DataType.DoubleWord,
                        OperandDefinitionKind.MemoryQuadWordReal => DataType.QuadWord,
                        OperandDefinitionKind.MemoryTenByteReal => throw new NotImplementedException(),

                        OperandDefinitionKind.RegisterByte => DataType.Byte,
                        OperandDefinitionKind.RegisterWord => DataType.Word,
                        OperandDefinitionKind.RegisterDoubleWord => DataType.DoubleWord,
                        OperandDefinitionKind.RegisterOrMemoryByte => DataType.Byte,
                        OperandDefinitionKind.RegisterOrMemoryWord => DataType.Word,
                        OperandDefinitionKind.RegisterOrMemoryDoubleWord => DataType.DoubleWord,
                        OperandDefinitionKind.RegisterOrMemoryQuadWord => DataType.QuadWord,

                        OperandDefinitionKind.ImmediateByte => DataType.Byte,
                        OperandDefinitionKind.ImmediateWord => DataType.Word,
                        OperandDefinitionKind.ImmediateDoubleWord => DataType.DoubleWord,

                        OperandDefinitionKind.TypePointer => FarPointerDataType | DataType.Pointer,

                        OperandDefinitionKind.TypeShort => DataType.Word,
                        OperandDefinitionKind.TypeDoubleWord => DataType.DoubleWord,
                        OperandDefinitionKind.TypeInt => DataType.DoubleWord,

                        OperandDefinitionKind.NearPointer => NearPointerDataType | DataType.Pointer,
                        OperandDefinitionKind.FarPointer => FarPointerDataType | DataType.Pointer,

                        OperandDefinitionKind.ShortLabel => DataType.Byte,
                        OperandDefinitionKind.LongLabel => DataType.Word,

                        OperandDefinitionKind.RAX => DataType.QuadWord,
                        OperandDefinitionKind.EAX => DataType.DoubleWord,
                        OperandDefinitionKind.AX => DataType.Word,
                        OperandDefinitionKind.AL => DataType.Byte,
                        OperandDefinitionKind.AH => DataType.Byte,

                        OperandDefinitionKind.RBX => DataType.QuadWord,
                        OperandDefinitionKind.EBX => DataType.DoubleWord,
                        OperandDefinitionKind.BX => DataType.Word,
                        OperandDefinitionKind.BL => DataType.Byte,
                        OperandDefinitionKind.BH => DataType.Byte,

                        OperandDefinitionKind.RCX => DataType.QuadWord,
                        OperandDefinitionKind.ECX => DataType.DoubleWord,
                        OperandDefinitionKind.CX => DataType.Word,
                        OperandDefinitionKind.CL => DataType.Byte,
                        OperandDefinitionKind.CH => DataType.Byte,

                        OperandDefinitionKind.RDX => DataType.QuadWord,
                        OperandDefinitionKind.EDX => DataType.DoubleWord,
                        OperandDefinitionKind.DX => DataType.Word,
                        OperandDefinitionKind.DL => DataType.Byte,
                        OperandDefinitionKind.DH => DataType.Byte,

                        OperandDefinitionKind.RSP => DataType.QuadWord,
                        OperandDefinitionKind.ESP => DataType.DoubleWord,
                        OperandDefinitionKind.SP => DataType.Word,

                        OperandDefinitionKind.RBP => DataType.QuadWord,
                        OperandDefinitionKind.EBP => DataType.DoubleWord,
                        OperandDefinitionKind.BP => DataType.Word,

                        OperandDefinitionKind.RSI => DataType.QuadWord,
                        OperandDefinitionKind.ESI => DataType.DoubleWord,
                        OperandDefinitionKind.SI => DataType.Word,

                        OperandDefinitionKind.RDI => DataType.QuadWord,
                        OperandDefinitionKind.EDI => DataType.DoubleWord,
                        OperandDefinitionKind.DI => DataType.Word,

                        OperandDefinitionKind.CS or
                        OperandDefinitionKind.DS or
                        OperandDefinitionKind.SS or
                        OperandDefinitionKind.ES or
                        OperandDefinitionKind.CR => DataType.Word,

                        OperandDefinitionKind.DR or
                        OperandDefinitionKind.TR or
                        OperandDefinitionKind.FS or
                        OperandDefinitionKind.GS => DataType.Word,

                        _ => DataType.None,
                    };
                    if (inferredType > bestInferredType)
                        bestInferredType = inferredType;
                }
            }
            else
                bestInferredType = DataWidthToDataType(entry.DataWidth);

            bool isDest = true;
            foreach (OperandDefinition sourceOp in entry.Operands)
            {
                Debug.Assert(opCount < targetOps.Length);

                DataType explicitType = DataType.None;

                // Operands to skip or to validate
                switch (sourceOp.Kind)
                {
                    case OperandDefinitionKind.KeywordFar:
                        Contract.Assert(entry.Flags.HasFlag(InstructionFlags.Far));
                        explicitType |= bestInferredType;
                        continue;

                    case OperandDefinitionKind.TypePointer:
                        Contract.Assert(entry.DataType.HasFlag(DataType.Pointer));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandDefinitionKind.TypeDoubleWord:
                        Contract.Assert(entry.DataType.HasFlag(DataType.DoubleWord));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandDefinitionKind.TypeInt:
                        Contract.Assert(entry.DataType.HasFlag(DataType.Int));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandDefinitionKind.TypeShort:
                        Contract.Assert(entry.DataType.HasFlag(DataType.Word));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandDefinitionKind.NearPointer:
                        Contract.Assert(!entry.Flags.HasFlag(InstructionFlags.Far) && entry.DataType.HasFlag(DataType.Pointer));
                        explicitType |= entry.DataType;
                        break; // We want to create an operand for this

                    case OperandDefinitionKind.FarPointer:
                        Contract.Assert(entry.Flags.HasFlag(InstructionFlags.Far) && entry.DataType.HasFlag(DataType.Pointer));
                        explicitType |= entry.DataType;
                        break; // We want to create an operand for this

                    default:
                        break;
                }

                byte register = 0;
                if (mode == ModType.RegisterMode)
                {
                    if (isDest || !hasRegField)
                        register = rmField;
                    else
                        register = regField;
                }
                else if (mode != ModType.Unknown)
                {
                    switch (sourceOp.Kind)
                    {
                        case OperandDefinitionKind.RegisterByte:
                        case OperandDefinitionKind.RegisterWord:
                        case OperandDefinitionKind.RegisterDoubleWord:
                        case OperandDefinitionKind.RegisterOrMemoryByte:
                        case OperandDefinitionKind.RegisterOrMemoryWord:
                        case OperandDefinitionKind.RegisterOrMemoryDoubleWord:
                        case OperandDefinitionKind.RegisterOrMemoryQuadWord:
                            if (!hasRegField)
                                register = rmField;
                            else
                                register = regField;
                            break;
                        default:
                            break;
                    }
                }



                DataType sourceType;
                if (sourceOp.DataType != DataType.None)
                    sourceType = sourceOp.DataType;
                else
                    sourceType = bestInferredType;

                InstructionOperand targetOp = CreateOperand(sourceOp, mode, register, eac, displacement, immediate, offset, segmentType, segmentAddress, sourceType);

                targetOps[opCount++] = targetOp;

                if (isDest)
                    isDest = false;
            }

            DataWidth dataWidth = entry.DataWidth;
            if (dataWidth.Type == DataWidthType.None)
                dataWidth = DataTypeToDataWidth(bestInferredType);

            int length = stream.Length - cur.Length;
            if (length > MaxInstructionLength)
                return new Error(ErrorCode.InstructionLengthTooLarge, $"The instruction length '{length}' of '{entry}' exceeds the max length of '{MaxInstructionLength}' bytes", position);
            if (length < entry.MinLength)
                return new Error(ErrorCode.InstructionLengthTooSmall, $"Expect instruction length to be at least '{entry.MinLength}' bytes, but got '{length}' bytes for instruction '{entry}'", position);
            if (length > entry.MaxLength)
                return new Error(ErrorCode.InstructionLengthTooLarge, $"The instruction length '{length}' of '{entry}' exceeds the max length of '{entry.MaxLength}' bytes", position);

            Span<InstructionOperand> actualOps = targetOps.Slice(0, opCount);
            return new Instruction(position, entry.Op, (byte)length, entry.Type, dataWidth, flags, actualOps);
        }

        public OneOf<Instruction, Error> TryDecodeNext(ReadOnlySpan<byte> stream, string streamName, uint position = 0)
        {
            ReadOnlySpan<byte> tmp = stream;
            OneOf<byte, Error> opCodeRes = ReadU8(ref tmp, streamName, position);
            if (opCodeRes.IsT1)
                return opCodeRes.AsT1;

            byte opCode = opCodeRes.AsT0;
            InstructionList instructionList = _entryTable[opCode];
            if (instructionList == null)
                return new Error(ErrorCode.OpCodeNotImplemented, $"Not implemented opcode '${opCode:X2}' / '{opCode.ToBinary()}'", position);
            else if ((byte)instructionList.Op != opCode)
                return new Error(ErrorCode.OpCodeMismatch, $"Mismatch opcode! Expect '${opCode:X2}', but got '{instructionList.Op}'", position);

            if (instructionList.Count == 1)
            {
                InstructionEntry entry = instructionList.First();
                OneOf<Instruction, Error> loadRes = DecodeInstruction(stream, streamName, position, entry);
                if (loadRes.TryPickT1(out Error error, out _))
                    return new Error(error, $"Failed to decode instruction '{entry}'", position);
                return loadRes.AsT0;
            }
            else
            {
                // Expect that there is at least one more byte
                foreach (InstructionEntry instructionEntry in instructionList)
                {
                    OneOf<Instruction, Error> loadRes = DecodeInstruction(stream, streamName, position, instructionEntry);
                    if (loadRes.TryPickT0(out Instruction instruction, out _))
                        return instruction;
                }
                return new Error(ErrorCode.OpCodeNotImplemented, $"No instruction entry found that matches the opcode '{opCode:X2}'", position);
            }
        }

        public static OneOf<AssemblyLine[], Error> GetAssemblyLines(IEnumerable<Instruction> instructions, OutputValueMode outputMode, string hexPrefix = "0x")
        {
            //
            // Second we convert the instructions into a hashtable, mapped by its position
            //
            Dictionary<uint, Instruction> positionToInstructionMap = instructions.ToDictionary(i => i.Position, i => i);

            //
            // Third we generate the entire jump labels
            //
            int labelCounter = 0;

            // Maps a instruction with a "myLabel:", so that other instructions can jump into it
            Dictionary<Instruction, string> instructionToSourceLabelMap = new Dictionary<Instruction, string>();

            // Maps a instruction as a jump to specific target label, e.g. JNE myLabel
            Dictionary<Instruction, string> instructionToTargetLabelMap = new Dictionary<Instruction, string>();

            foreach (Instruction instruction in instructions)
            {
                switch (instruction.Type)
                {
                    case InstructionType.CALL:
                    case InstructionType.JMP:
                    case InstructionType.JA:
                    case InstructionType.JAE:
                    case InstructionType.JB:
                    case InstructionType.JBE:
                    case InstructionType.JC:
                    case InstructionType.JCXZ:
                    case InstructionType.JE:
                    case InstructionType.JG:
                    case InstructionType.JGE:
                    case InstructionType.JL:
                    case InstructionType.JLE:
                    case InstructionType.JNC:
                    case InstructionType.JNE:
                    case InstructionType.JNO:
                    case InstructionType.JNS:
                    case InstructionType.JNP:
                    case InstructionType.JO:
                    case InstructionType.JP:
                    case InstructionType.JS:
                    case InstructionType.LOOP:
                    case InstructionType.LOOPE:
                    case InstructionType.LOOPNZ:
                        {
                            if (instruction.Operands.Length != 1)
                                return new Error(ErrorCode.InvalidOperandsLength, $"Expect number of operands to be 1, but got '{instruction.Operands.Length}' for jump instruction '{instruction}'", instruction.Position);

                            InstructionOperand firstOp = instruction.Operands[0];

                            uint absoluteAddress = 0;
                            if (firstOp.Type == OperandType.Immediate)
                            {
                                if (!firstOp.Immediate.Flags.HasFlag(ImmediateFlag.RelativeJumpDisplacement))
                                    return new Error(ErrorCode.UnsupportedImmediateFlags, $"The immediate '{firstOp.Immediate}' is not a relative jump displacement for jump instruction '{instruction}'", instruction.Position);
                                int relativeAddressAfterThisInstruction = firstOp.Immediate.Value;
                                absoluteAddress = (uint)(instruction.Position + instruction.Length + relativeAddressAfterThisInstruction);
                            }
                            else if (firstOp.Type == OperandType.Value)
                            {
                                int relativeAddressAfterThisInstruction = firstOp.Value;
                                absoluteAddress = (uint)(instruction.Position + instruction.Length + relativeAddressAfterThisInstruction);
                            }
                            else
                                break;

                            if (!positionToInstructionMap.TryGetValue(absoluteAddress, out Instruction instructionToJumpTo))
                                return new Error(ErrorCode.JumpInstructionNotFound, $"No instruction for absolute address '{absoluteAddress}' found for jump instruction '{instruction}'", instruction.Position);

                            if (!instructionToSourceLabelMap.TryGetValue(instructionToJumpTo, out string label))
                            {
                                label = $"label{labelCounter++}";
                                instructionToSourceLabelMap.Add(instructionToJumpTo, label);
                            }

                            instructionToTargetLabelMap.Add(instruction, label);
                        }
                        break;
                }
            }

            List<AssemblyLine> result = new List<AssemblyLine>();
            Queue<Mnemonic> prefixInstructions = new Queue<Mnemonic>();
            foreach (Instruction instruction in instructions)
            {
                if (instruction.Flags.HasFlag(InstructionFlags.Prefix))
                    prefixInstructions.Enqueue(instruction.Mnemonic);

                if (instructionToSourceLabelMap.TryGetValue(instruction, out string sourceLabel))
                    result.Add(new AssemblyLine(instruction.Position, AssemblyLineType.SourceLabel, instruction.Mnemonic, null, sourceLabel));

                if (instructionToTargetLabelMap.TryGetValue(instruction, out string targetLabel))
                {
                    result.Add(new AssemblyLine(instruction.Position, AssemblyLineType.TargetLabel, instruction.Mnemonic, null, targetLabel));
                    continue;
                }

                if (instruction.Flags.HasFlag(InstructionFlags.Prefix))
                    continue;

                if (prefixInstructions.Count > 0)
                {
                    StringBuilder asm = new StringBuilder();
                    bool hasLock = false;
                    while (prefixInstructions.TryDequeue(out Mnemonic prefix))
                    {
                        if (asm.Length > 0)
                            asm.Append(' ');
                        asm.Append(prefix.Name);
                        if (prefix.Type == InstructionType.LOCK)
                            hasLock |= true;
                    }

                    if (hasLock && instruction.Type == InstructionType.XCHG)
                    {
                        // NOTE(@final): LOCK XCHG requires the operands to be reversed in the assembly output
                        InstructionOperand tmp = instruction.Operands[0];
                        instruction.Operands[0] = instruction.Operands[1];
                        instruction.Operands[1] = tmp;
                    }

                    string insAsm = instruction.Asm(outputMode, hexPrefix);

                    asm.Append(' ');
                    asm.Append(insAsm);
                    result.Add(new AssemblyLine(instruction.Position, AssemblyLineType.Default, instruction.Mnemonic, asm.ToString(), null));
                }
                else
                {
                    string asm = instruction.Asm(outputMode, hexPrefix);
                    result.Add(new AssemblyLine(instruction.Position, AssemblyLineType.Default, instruction.Mnemonic, asm, null));
                }

                prefixInstructions.Clear();
            }
            return result.ToArray();
        }

        public OneOf<string, Error> GetAssembly(ReadOnlySpan<byte> stream, string streamName, OutputValueMode outputMode, string hexPrefix = "0x")
        {
            //
            // First we decode all instructions and store it in a list
            //
            List<Instruction> instructions = new List<Instruction>();
            ReadOnlySpan<byte> cur = stream;
            uint position = 0;
            while (cur.Length > 0)
            {
                OneOf<Instruction, Error> decodeRes = TryDecodeNext(cur, streamName, position);
                if (decodeRes.IsT1)
                    return decodeRes.AsT1;

                Instruction instruction = decodeRes.AsT0;
                instructions.Add(instruction);

                cur = cur.Slice(instruction.Length);
                position += instruction.Length;
            }

            //
            // Second generate the assembly lines, that may contain additional lines for labels or swapped label jumps
            //
            OneOf<AssemblyLine[], Error> linesRes = GetAssemblyLines(instructions, outputMode, hexPrefix);
            if (linesRes.IsT1)
                return linesRes.AsT1;

            var lines = linesRes.AsT0;

            //
            // Lastly we generate the assembly for each instruction and insert source labels before or replace the entire instruction with a label jump
            //
            StringBuilder s = new StringBuilder();
            s.AppendLine("; ========================================================================");
            s.AppendLine("; 8086 CPU Simulator");
            s.AppendLine("; © 2023 by Torsten Spaete");
            s.AppendLine("; ========================================================================");
            s.AppendLine(";");
            s.Append("; ");
            s.AppendLine(streamName);
            s.AppendLine();
            s.AppendLine("bits 16");
            s.AppendLine();

            foreach (AssemblyLine line in lines)
                s.AppendLine(line.ToString());

            return s.ToString();
        }

        private OneOf<int, Error> ExecuteInstruction(Instruction instruction, IRunState state)
        {
            if (instruction == null)
                return new Error(ErrorCode.MissingInstructionParameter, $"The instruction parameter is missing!", 0);

            uint cycles = ComputeCycles(instruction);
            int ms = (int)Math.Round(cycles * MillisecondPerCycle) + 1;
            Thread.Sleep(ms);

            return _executer.Execute(instruction, state);
        }

        private static bool IsFinishedInstruction(Instruction instruction)
        {
            if (instruction == null)
                return false;
            InstructionType type = instruction.Type;
            return type switch
            {
                InstructionType.HLT => true,
                _ => false
            };
        }

        private static uint GetCodeEnd(uint length)
        {
            uint codeStart = CodeSegmentStart;
            uint codeEnd = codeStart + length;
            return codeEnd;
        }

        private ReadOnlySpan<byte> GetCodeStream(uint ip, uint length)
        {
            uint codeEnd = GetCodeEnd(length);
            uint codeOffset = GetAbsoluteMemoryAddress(new MemoryAddress(EffectiveAddressCalculation.DirectAddress, (int)ip, SegmentType.CS, 0));
            if (codeEnd > codeOffset)
            {
                uint codeLen = codeEnd - codeOffset;
                ReadOnlySpan<byte> stream = Memory.Get(codeOffset, codeLen);
                return stream;
            }
            return ReadOnlySpan<byte>.Empty;
        }

        public OneOf<uint, Error> BeginStepping()
        {
            if (ActiveProgram == null)
                return new Error(ErrorCode.ProgramNotLoaded, $"No program was loaded", 0);

            if (!(ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Finished || ExecutionState == ExecutionState.Failed))
                return new Error(ErrorCode.InvalidExecutionState, $"The execution state '{ExecutionState}' is not valid for {nameof(BeginStepping)}", 0);

            ExecutionState = ExecutionState.Halted;

            if (ActiveProgram.Register != null)
                Register.Assign(ActiveProgram.Register);
            else
                Register.Reset();

            CurrentIP = PreviousIP = Register.IP;
            CurrentInstruction = null;

            ReadOnlySpan<byte> stream = GetCodeStream(CurrentIP, (uint)ActiveProgram.Length);
            if (stream.Length == 0)
                return new Error(ErrorCode.EndOfStream, $"The code stream at IP '{CurrentIP}' is empty", CurrentIP);

            Thread.Sleep((int)InstructionDecodeMilliseconds);
            OneOf<Instruction, Error> decodeRes = TryDecodeNext(stream, ActiveProgram.Name, CurrentIP);
            if (decodeRes.IsT1)
            {
                ExecutionState = ExecutionState.Failed;
                return decodeRes.AsT1;
            }

            CurrentInstruction = decodeRes.AsT0;

            return CurrentIP;
        }

        public void StopStepping()
        {
            if (ExecutionState == ExecutionState.Halted)
            {
                ExecutionState = ExecutionState.Stopped;
                PreviousIP = CurrentIP = uint.MaxValue;
                CurrentInstruction = null;
            }
        }

        public OneOf<Instruction, Error> Step(RunState state)
        {
            if (state == null)
                return new Error(ErrorCode.MissingStateParameter, $"The state argument is missing", 0);

            if (ActiveProgram == null)
                return new Error(ErrorCode.ProgramNotLoaded, $"No program was loaded", 0);

            if (ExecutionState != ExecutionState.Halted)
                return new Error(ErrorCode.InvalidExecutionState, $"The execution state '{ExecutionState}' is not valid for {nameof(Step)}", CurrentIP);

            PreviousIP = CurrentIP;

            ExecutionState = ExecutionState.Running;

            ReadOnlySpan<byte> stream = GetCodeStream(CurrentIP, (uint)ActiveProgram.Length);
            if (stream.Length == 0)
                return new Error(ErrorCode.EndOfStream, $"The code stream at IP '{CurrentIP}' is empty", CurrentIP);

            Thread.Sleep((int)InstructionDecodeMilliseconds);
            OneOf<Instruction, Error> decodeRes = TryDecodeNext(stream, ActiveProgram.Name, CurrentIP);
            if (decodeRes.IsT1)
            {
                ExecutionState = ExecutionState.Failed;
                return decodeRes.AsT1;
            }

            uint programEnd = (uint)ActiveProgram.Length;

            Instruction instruction = decodeRes.AsT0;

            CurrentIP += instruction.Length;
            Register.IP = (ushort)CurrentIP;

            CurrentInstruction = instruction;

            OneOf<int, Error> executionRes = ExecuteInstruction(instruction, state);
            if (executionRes.IsT1)
            {
                ExecutionState = ExecutionState.Failed;
                return executionRes.AsT1;
            }

            uint newIp = (ushort)(CurrentIP + executionRes.AsT0);
            CurrentIP = newIp;
            Register.IP = (ushort)CurrentIP;

            if ((CurrentIP == programEnd) || IsFinishedInstruction(instruction))
            {
                ExecutionState = ExecutionState.Finished;
                CurrentIP = uint.MaxValue;
                PreviousIP = uint.MaxValue;
                CurrentInstruction = null;
            }
            else
            {
                ExecutionState = ExecutionState.Halted;
                PreviousIP = CurrentIP;
            }

            return CurrentInstruction;
        }

        public OneOf<uint, Error> Run(RunState state)
        {
            if (state == null)
                return new Error(ErrorCode.MissingStateParameter, $"The state argument is missing", 0);

            if (ActiveProgram == null)
                return new Error(ErrorCode.ProgramNotLoaded, $"No program was loaded", 0);
            if (!(ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Finished || ExecutionState == ExecutionState.Failed))
                return new Error(ErrorCode.InvalidExecutionState, $"The execution state '{ExecutionState}' is not valid for {nameof(Run)}", 0);

            uint result = 0;

            ExecutionState = ExecutionState.Running;

            if (ActiveProgram.Register != null)
                Register.Assign(ActiveProgram.Register);
            else
                Register.Reset();

            CurrentIP = PreviousIP = Register.IP;
            CurrentInstruction = null;

            uint ipEnd = (uint)ActiveProgram.Length;

            while (!state.IsStopped)
            {
                if ((CurrentIP == ipEnd) || IsFinishedInstruction(CurrentInstruction))
                    break;

                uint ip = PreviousIP = CurrentIP;
                Contract.Assert(ip < ActiveProgram.Length);

                ReadOnlySpan<byte> stream = GetCodeStream(CurrentIP, (uint)ActiveProgram.Length);
                if (stream.Length == 0)
                    return new Error(ErrorCode.EndOfStream, $"The code stream at IP '{CurrentIP}' is empty", CurrentIP);

                Thread.Sleep((int)InstructionDecodeMilliseconds);
                OneOf<Instruction, Error> decodeRes = TryDecodeNext(stream, ActiveProgram.Name, ip);
                if (decodeRes.IsT1)
                {
                    ExecutionState = ExecutionState.Failed;
                    return decodeRes.AsT1;
                }

                Instruction instruction = decodeRes.AsT0;

                CurrentIP += instruction.Length;
                Contract.Assert(CurrentIP < ushort.MaxValue);
                Register.IP = (ushort)CurrentIP;

                CurrentInstruction = instruction;

                OneOf<int, Error> executionRes = ExecuteInstruction(instruction, state);
                if (executionRes.IsT1)
                {
                    ExecutionState = ExecutionState.Failed;
                    return executionRes.AsT1;
                }

                uint newIp = (ushort)(CurrentIP + executionRes.AsT0);
                CurrentIP = newIp;
                Contract.Assert(CurrentIP < ushort.MaxValue);
                Register.IP = (ushort)CurrentIP;
            }

            if (state.IsStopped)
                ExecutionState = ExecutionState.Stopped;
            else
                ExecutionState = ExecutionState.Finished;

            PreviousIP = CurrentIP = uint.MaxValue;
            CurrentInstruction = null;

            return result;
        }
    }
}
