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
    public class CPU : INotifyPropertyChanged
    {
        private const int MaxInstructionLength = 6;

        private static readonly RegisterTable _regTable = new RegisterTable();
        private static readonly EffectiveAddressCalculationTable _effectiveAddressCalculationTable = new EffectiveAddressCalculationTable();

        private readonly InstructionTable _entryTable = new InstructionTable();

        private readonly InstructionExecuter _executer;

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly DataWidth PointerDataWidth = new DataWidth(DataWidthType.Word);
        private static readonly DataType PointerDataType = DataType.Word;

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

        public MemoryTable Memory { get; }

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

            Memory = new MemoryTable();
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

            ActiveProgram = program;

            if (program.Register != null)
                Register.Assign(program.Register);
            else
                Register.Reset();
            RaisePropertyChanged(nameof(Register));

            Memory.Clear();
            RaisePropertyChanged(nameof(Memory));

            return program.Length;
        }

        public OneOf<Immediate, Error> LoadRegister(RegisterType type)
        {
            return type switch
            {
                RegisterType.AX => new Immediate(Register.AX, ImmediateFlag.None),
                RegisterType.AL => new Immediate(Register.AL, ImmediateFlag.None),
                RegisterType.AH => new Immediate(Register.AH, ImmediateFlag.None),

                RegisterType.BX => new Immediate(Register.BX, ImmediateFlag.None),
                RegisterType.BL => new Immediate(Register.BL, ImmediateFlag.None),
                RegisterType.BH => new Immediate(Register.BH, ImmediateFlag.None),

                RegisterType.CX => new Immediate(Register.CX, ImmediateFlag.None),
                RegisterType.CL => new Immediate(Register.CL, ImmediateFlag.None),
                RegisterType.CH => new Immediate(Register.CH, ImmediateFlag.None),

                RegisterType.DX => new Immediate(Register.DX, ImmediateFlag.None),
                RegisterType.DL => new Immediate(Register.DL, ImmediateFlag.None),
                RegisterType.DH => new Immediate(Register.DH, ImmediateFlag.None),

                RegisterType.SP => new Immediate(Register.SP, ImmediateFlag.None),
                RegisterType.BP => new Immediate(Register.BP, ImmediateFlag.None),
                RegisterType.SI => new Immediate(Register.SI, ImmediateFlag.None),
                RegisterType.DI => new Immediate(Register.DI, ImmediateFlag.None),

                RegisterType.CS => new Immediate(Register.CS, ImmediateFlag.None),
                RegisterType.DS => new Immediate(Register.DS, ImmediateFlag.None),
                RegisterType.SS => new Immediate(Register.SS, ImmediateFlag.None),
                RegisterType.ES => new Immediate(Register.ES, ImmediateFlag.None),

                _ => new Error(ErrorCode.UnsupportedRegisterType, $"The register type '{type}' is not supported", 0),
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

        public OneOf<byte, Error> StoreRegister(RegisterType type, Immediate value)
        {
            byte result;
            switch (type)
            {
                case RegisterType.AX:
                    Register.AX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.AL:
                    Register.AL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.AH:
                    Register.AH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.BX:
                    Register.BX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.BL:
                    Register.BL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.BH:
                    Register.BH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.CX:
                    Register.CX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.CL:
                    Register.CL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.CH:
                    Register.CH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.DX:
                    Register.DX = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.DL:
                    Register.DL = ImmediateToS8(value);
                    result = 1;
                    break;
                case RegisterType.DH:
                    Register.DH = ImmediateToS8(value);
                    result = 1;
                    break;

                case RegisterType.SP:
                    Register.SP = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.BP:
                    Register.BP = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.SI:
                    Register.SI = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.DI:
                    Register.DI = ImmediateToS16(value);
                    result = 2;
                    break;

                case RegisterType.CS:
                    Register.CS = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.DS:
                    Register.DS = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.SS:
                    Register.SS = ImmediateToS16(value);
                    result = 2;
                    break;
                case RegisterType.ES:
                    Register.ES = ImmediateToS16(value);
                    result = 2;
                    break;

                default:
                    return new Error(ErrorCode.UnsupportedRegisterType, $"The register type '{type}' is not supported", 0);
            }

            return result;
        }

        private const int HighestMemoryAddress = 0xFFFFF;

        private const int EndExtraSegmentStart = 0x7FFFF;
        private const int StartExtraSegmentStart = 0x70000;

        private const int EndStackSegmentStart = 0x5FFFF;
        private const int StartStackSegmentStart = 0x50000;

        private const int EndCodeSegmentStart = 0x3FFFF;
        private const int StartCodeSegmentStart = 0x30000;

        private const int EndDataSegmentStart = 0x2FFFF;
        private const int StartDataSegmentStart = 0x20000;

        private int GetDataTypeSize(DataType type)
        {
            if (type == DataType.Pointer)
                type = PointerDataType;
            return type switch
            {
                DataType.Byte => 1,
                DataType.Word => 2,
                DataType.Int => 4,
                DataType.DoubleWord => 4,
                DataType.QuadWord => 8,
                DataType.Pointer => 8,
                _ => 0,
            };
        }

        private int GetAbsoluteMemoryAddress(MemoryAddress address)
        {
            byte u8 = (byte)(address.Displacement & 0xFF);
            ushort u16 = (ushort)(address.Displacement & 0xFFFF);
            int d8 = (u8 & 0b10000000) == 0b10000000 ? (-u8) : u8;
            int d16 = (u16 & 0b10000000_00000000) == 0b10000000_00000000 ? (-u16) : u16;
            int result = address.EAC switch
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
            return result;
        }

        private OneOf<Immediate, Error> LoadMemory(int absoluteAddress, DataType type)
        {
            int typeSize = GetDataTypeSize(type);
            if (absoluteAddress < 0 || (absoluteAddress + typeSize) >= Memory.Length)
                return new Error(ErrorCode.InvalidMemoryAddress, $"The absolute source memory address '{absoluteAddress}' is not valid for type '{type}'!", 0);

            switch (type)
            {
                case DataType.Byte:
                    return new Immediate(Memory[absoluteAddress], ImmediateFlag.None);

                case DataType.Word:
                    {
                        byte low = Memory[absoluteAddress + 0];
                        byte high = Memory[absoluteAddress + 1];
                        ushort u16 = (ushort)(low | (high << 8));
                        return new Immediate(u16, ImmediateFlag.None);
                    }

                case DataType.Pointer:
                    {
                        if (PointerDataType == DataType.Word)
                        {
                            byte low = Memory[absoluteAddress + 0];
                            byte high = Memory[absoluteAddress + 1];
                            ushort u16 = (ushort)(low | (high << 8));
                            return new Immediate(u16, ImmediateFlag.None);
                        }
                        else
                            return new Error(ErrorCode.UnsupportedDataType, $"The pointer data type '{PointerDataType}' is not supported", 0);
                    }

                default:
                    {
                        if (type.HasFlag(DataType.Pointer))
                        {
                            DataType pointerDataType;
                            if (type.HasFlag(DataType.Byte))
                                pointerDataType = DataType.Byte;
                            else if (type.HasFlag(DataType.Word))
                                pointerDataType = DataType.Word;
                            else if (type == DataType.Pointer)
                                pointerDataType = PointerDataType;
                            else
                                return new Error(ErrorCode.UnsupportedDataType, $"The source pointer memory type '{type}' is not supported!", 0);
                            return LoadMemory(absoluteAddress, pointerDataType);
                        }
                        else
                            return new Error(ErrorCode.UnsupportedDataType, $"The source memory type '{type}' is not supported!", 0);
                    }
            }
        }

        public OneOf<Immediate, Error> LoadMemory(MemoryAddress address, DataType type)
        {
            int absoluteAddress = GetAbsoluteMemoryAddress(address);
            if (absoluteAddress == int.MinValue)
                return new Error(ErrorCode.UnsupportedEffectiveAddressCalculation, $"The effective address calculation '{address.EAC}' is not supported for the specified memory address '{address}' for type '{type}'", 0);
            return LoadMemory(absoluteAddress, type);
        }

        private OneOf<byte, Error> StoreMemory(int absoluteAddress, DataType type, Immediate value)
        {
            int typeSize = GetDataTypeSize(type);
            if (absoluteAddress < 0 || (absoluteAddress + typeSize) >= Memory.Length)
                return new Error(ErrorCode.InvalidMemoryAddress, $"The absolute destination memory address '{absoluteAddress}' is not valid for type '{type}'!", 0);
            switch (type)
            {
                case DataType.Byte:
                    Memory[absoluteAddress] = value.U8;
                    return 1;
                case DataType.Word:
                    {
                        ushort u16 = value.U16;
                        Memory[absoluteAddress + 0] = (byte)((u16 >> 0) & 0xFF);
                        Memory[absoluteAddress + 1] = (byte)((u16 >> 8) & 0xFF);
                        return 2;
                    }
                default:
                    return new Error(ErrorCode.UnsupportedDataWidth, $"The destination memory type '{type}' is not supported!", 0);
            }
        }

        public OneOf<byte, Error> StoreMemory(MemoryAddress address, DataType type, Immediate value)
        {
            int absoluteAddress = GetAbsoluteMemoryAddress(address);
            if (absoluteAddress == int.MinValue)
                return new Error(ErrorCode.UnsupportedEffectiveAddressCalculation, $"The effective address calculation '{address.EAC}' is not supported for the specified memory address '{address}' for type '{type}'", 0);
            return StoreMemory(absoluteAddress, type, value);
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

        static InstructionOperand CreateOperand(Operand sourceOp, ModType mode, byte registerBits, EffectiveAddressCalculation eac, int displacement, int immediate, int offset, int segment, DataType type)
        {
            switch (sourceOp.Kind)
            {
                case OperandKind.Value:
                    return new InstructionOperand(s32: sourceOp.Value);

                case OperandKind.MemoryByte:
                    return new InstructionOperand(new MemoryAddress(eac, displacement));

                case OperandKind.MemoryWord:
                    return new InstructionOperand(new MemoryAddress(eac, displacement));

                case OperandKind.MemoryDoubleWord:
                    return new InstructionOperand(new MemoryAddress(eac, displacement));

                case OperandKind.MemoryQuadWord:
                    return new InstructionOperand(new MemoryAddress(eac, displacement));

                case OperandKind.MemoryWordReal:
                case OperandKind.MemoryDoubleWordReal:
                case OperandKind.MemoryQuadWordReal:
                case OperandKind.MemoryTenByteReal:
                    break;

                case OperandKind.SourceRegister:
                    {
                        if (type == DataType.Byte)
                            return new InstructionOperand(_regTable.GetByte(registerBits));
                        else if (type == DataType.Word)
                            return new InstructionOperand(_regTable.GetWord(registerBits));
                        else
                            throw new NotSupportedException($"Unsupported type of '{type}' for source register");
                    }

                case OperandKind.RegisterByte:
                    return new InstructionOperand(_regTable.GetByte(registerBits));
                case OperandKind.RegisterWord:
                    return new InstructionOperand(_regTable.GetWord(registerBits));
                case OperandKind.RegisterDoubleWord:
                    break;

                case OperandKind.RegisterOrMemoryByte:
                    if (mode == ModType.RegisterMode)
                        return new InstructionOperand(_regTable.GetByte(registerBits));
                    else
                        return new InstructionOperand(eac, displacement);
                case OperandKind.RegisterOrMemoryWord:
                    if (mode == ModType.RegisterMode)
                        return new InstructionOperand(_regTable.GetWord(registerBits));
                    else
                        return new InstructionOperand(eac, displacement);
                case OperandKind.RegisterOrMemoryDoubleWord:
                case OperandKind.RegisterOrMemoryQuadWord:
                    break;

                case OperandKind.ImmediateByte:
                    if ((sbyte)immediate < 0)
                        return new InstructionOperand((sbyte)(immediate & 0xFF), ImmediateFlag.None);
                    else
                        return new InstructionOperand((byte)(immediate & 0xFF), ImmediateFlag.None);
                case OperandKind.ImmediateWord:
                    if ((short)immediate < 0)
                        return new InstructionOperand((short)(immediate & 0xFFFF), ImmediateFlag.None);
                    else
                        return new InstructionOperand((ushort)(immediate & 0xFFFF), ImmediateFlag.None);
                case OperandKind.ImmediateDoubleWord:
                    if ((int)immediate < 0)
                        return new InstructionOperand((int)(immediate & 0xFFFFFFFF), ImmediateFlag.None);
                    else
                        return new InstructionOperand((uint)(immediate & 0xFFFFFFFF), ImmediateFlag.None);

                case OperandKind.TypeDoubleWord:
                    break;
                case OperandKind.TypeShort:
                    break;
                case OperandKind.TypeInt:
                    break;
                case OperandKind.TypePointer:
                    break;

                case OperandKind.ShortLabel:
                    if ((sbyte)displacement < 0)
                        return new InstructionOperand((sbyte)(displacement & 0xFF), ImmediateFlag.RelativeJumpDisplacement);
                    else
                        return new InstructionOperand((byte)(displacement & 0xFF), ImmediateFlag.RelativeJumpDisplacement);
                case OperandKind.LongLabel:
                    if ((short)displacement < 0)
                        return new InstructionOperand((short)(displacement & 0xFFFF), ImmediateFlag.RelativeJumpDisplacement);
                    else
                        return new InstructionOperand((ushort)(displacement & 0xFFFF), ImmediateFlag.RelativeJumpDisplacement);

                case OperandKind.FarPointer:
                    {
                        int address = offset + segment;
                        if ((short)segment < 0)
                            return new InstructionOperand(new MemoryAddress((short)(address & 0xFFFF)));
                        else
                            return new InstructionOperand(new MemoryAddress((ushort)(address & 0xFFFF)));
                    }

                case OperandKind.NearPointer:
                    {
                        int address = offset + segment;
                        if ((sbyte)address < 0)
                            return new InstructionOperand(new MemoryAddress((sbyte)(address & 0xFF)));
                        else
                            return new InstructionOperand(new MemoryAddress((byte)(address & 0xFF)));
                    }

                case OperandKind.ST:
                    break;
                case OperandKind.ST_I:
                    break;
                case OperandKind.M:
                    break;
                case OperandKind.M_Number:
                    break;

                case OperandKind.RAX:
                    return new InstructionOperand(RegisterType.RAX);
                case OperandKind.EAX:
                    return new InstructionOperand(RegisterType.EAX);
                case OperandKind.AX:
                    return new InstructionOperand(RegisterType.AX);
                case OperandKind.AL:
                    return new InstructionOperand(RegisterType.AL);
                case OperandKind.AH:
                    return new InstructionOperand(RegisterType.AH);

                case OperandKind.RBX:
                    return new InstructionOperand(RegisterType.RBX);
                case OperandKind.EBX:
                    return new InstructionOperand(RegisterType.EBX);
                case OperandKind.BX:
                    return new InstructionOperand(RegisterType.BX);
                case OperandKind.BL:
                    return new InstructionOperand(RegisterType.BL);
                case OperandKind.BH:
                    return new InstructionOperand(RegisterType.BH);

                case OperandKind.RCX:
                    return new InstructionOperand(RegisterType.RCX);
                case OperandKind.ECX:
                    return new InstructionOperand(RegisterType.ECX);
                case OperandKind.CX:
                    return new InstructionOperand(RegisterType.CX);
                case OperandKind.CL:
                    return new InstructionOperand(RegisterType.CL);
                case OperandKind.CH:
                    return new InstructionOperand(RegisterType.CH);

                case OperandKind.RDX:
                    return new InstructionOperand(RegisterType.RDX);
                case OperandKind.EDX:
                    return new InstructionOperand(RegisterType.EDX);
                case OperandKind.DX:
                    return new InstructionOperand(RegisterType.DX);
                case OperandKind.DL:
                    return new InstructionOperand(RegisterType.DL);
                case OperandKind.DH:
                    return new InstructionOperand(RegisterType.DH);

                case OperandKind.RSP:
                    return new InstructionOperand(RegisterType.RSP);
                case OperandKind.ESP:
                    return new InstructionOperand(RegisterType.ESP);
                case OperandKind.SP:
                    return new InstructionOperand(RegisterType.SP);

                case OperandKind.RBP:
                    return new InstructionOperand(RegisterType.RBP);
                case OperandKind.EBP:
                    return new InstructionOperand(RegisterType.EBP);
                case OperandKind.BP:
                    return new InstructionOperand(RegisterType.BP);

                case OperandKind.RSI:
                    return new InstructionOperand(RegisterType.RSI);
                case OperandKind.ESI:
                    return new InstructionOperand(RegisterType.ESI);
                case OperandKind.SI:
                    return new InstructionOperand(RegisterType.SI);

                case OperandKind.RDI:
                    return new InstructionOperand(RegisterType.RDI);
                case OperandKind.EDI:
                    return new InstructionOperand(RegisterType.EDI);
                case OperandKind.DI:
                    return new InstructionOperand(RegisterType.DI);

                case OperandKind.CS:
                    return new InstructionOperand(RegisterType.CS);
                case OperandKind.DS:
                    return new InstructionOperand(RegisterType.DS);
                case OperandKind.SS:
                    return new InstructionOperand(RegisterType.SS);
                case OperandKind.ES:
                    return new InstructionOperand(RegisterType.ES);

                case OperandKind.CR:
                    return new InstructionOperand(RegisterType.CR);
                case OperandKind.DR:
                    return new InstructionOperand(RegisterType.DR);
                case OperandKind.TR:
                    return new InstructionOperand(RegisterType.TR);
                case OperandKind.FS:
                    return new InstructionOperand(RegisterType.FS);
                case OperandKind.GS:
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
                DataType.Pointer => PointerDataWidth,
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
            int segment = 0;

            ModType mode = ModType.Unknown;

            EffectiveAddressCalculation eac = EffectiveAddressCalculation.None;

            int displacementLength = 0;

            bool hasRegField = false;

            foreach (Field field in entry.Fields)
            {
                switch (field.Type)
                {
                    case FieldType.Constant:
                        {
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return value.AsT1;
                            if (field.Value != value.AsT0)
                                return new Error(ErrorCode.ConstantFieldMismatch, $"Expect constant to be '{field.Value}', but got instead '{value.AsT0}' in field '{field}'", position);
                        }
                        break;
                    case FieldType.ModRegRM:
                    case FieldType.Mod000RM:
                    case FieldType.Mod001RM:
                    case FieldType.Mod010RM:
                    case FieldType.Mod011RM:
                    case FieldType.Mod100RM:
                    case FieldType.Mod101RM:
                    case FieldType.Mod110RM:
                    case FieldType.Mod111RM:
                        {
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return value.AsT1;
                            byte mrm = value.AsT0;
                            modField = (byte)(mrm >> 6 & 0b111);
                            regField = (byte)(mrm >> 3 & 0b111);
                            rmField = (byte)(mrm >> 0 & 0b111);
                            if (field.Type != FieldType.ModRegRM)
                            {
                                hasRegField = false;
                                byte expectReg = field.Type - FieldType.Mod000RM;
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
                    case FieldType.Displacement0:
                    case FieldType.Displacement1:
                        {
                            int t = field.Type - FieldType.Displacement0;
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
                    case FieldType.Immediate0:
                    case FieldType.Immediate1:
                    case FieldType.Immediate2:
                    case FieldType.Immediate3:
                        {
                            int t = (int)field.Type - (int)FieldType.Immediate0;
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the immediate-{t} in field '{field}'", position);
                            byte imm8 = value.AsT0;
                            int shift = t * 8;
                            immediate |= ((int)imm8 << shift);
                        }
                        break;

                    case FieldType.RelativeLabelDisplacement0:
                    case FieldType.RelativeLabelDisplacement1:
                        {
                            int t = field.Type - FieldType.RelativeLabelDisplacement0;
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the relative label displacement-{t} in field '{field}'", position);
                            byte d8 = value.AsT0;
                            int shift = t * 8;
                            displacement |= ((int)d8 << shift);
                        }
                        break;

                    case FieldType.Immediate0to3:
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

                    case FieldType.Offset0:
                    case FieldType.Offset1:
                        {
                            int t = (int)(field.Type - FieldType.Offset0);
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the offset-{t} in field '{field}'", position);
                            byte offset8 = value.AsT0;
                            int shift = t * 8;
                            offset |= ((int)offset8 << shift);
                        }
                        break;

                    case FieldType.Segment0:
                    case FieldType.Segment1:
                        {
                            int t = (int)(field.Type - FieldType.Segment0);
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the segment-{t} in field '{field}'", position);
                            byte seg8 = value.AsT0;
                            int shift = t * 8;
                            segment |= ((int)seg8 << shift);
                        }
                        break;

                    case FieldType.ShortLabelOrShortLow:
                        {
                            OneOf<byte, Error> value = ReadU8(ref cur, streamName, position);
                            if (value.IsT1)
                                return new Error(value.AsT1, $"No more bytes left for reading the low displacement in field '{field}'", position);
                            byte low = value.AsT0;
                            displacement |= ((int)low << 0);
                        }
                        break;

                    case FieldType.LongLabel:
                    case FieldType.ShortHigh:
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
                foreach (Operand sourceOp in entry.Operands)
                {
                    DataType inferredType = sourceOp.Kind switch
                    {
                        OperandKind.MemoryByte => DataType.Byte,
                        OperandKind.MemoryWord => DataType.Word,
                        OperandKind.MemoryDoubleWord => DataType.DoubleWord,
                        OperandKind.MemoryQuadWord => DataType.QuadWord,

                        OperandKind.MemoryWordReal => DataType.Word,
                        OperandKind.MemoryDoubleWordReal => DataType.DoubleWord,
                        OperandKind.MemoryQuadWordReal => DataType.QuadWord,
                        OperandKind.MemoryTenByteReal => throw new NotImplementedException(),

                        OperandKind.RegisterByte => DataType.Byte,
                        OperandKind.RegisterWord => DataType.Word,
                        OperandKind.RegisterDoubleWord => DataType.DoubleWord,
                        OperandKind.RegisterOrMemoryByte => DataType.Byte,
                        OperandKind.RegisterOrMemoryWord => DataType.Word,
                        OperandKind.RegisterOrMemoryDoubleWord => DataType.DoubleWord,
                        OperandKind.RegisterOrMemoryQuadWord => DataType.QuadWord,

                        OperandKind.ImmediateByte => DataType.Byte,
                        OperandKind.ImmediateWord => DataType.Word,
                        OperandKind.ImmediateDoubleWord => DataType.DoubleWord,

                        OperandKind.TypePointer => DataType.Pointer,

                        OperandKind.TypeShort => DataType.Word,
                        OperandKind.TypeDoubleWord => DataType.DoubleWord,
                        OperandKind.TypeInt => DataType.DoubleWord,

                        OperandKind.NearPointer => DataType.Pointer,
                        OperandKind.FarPointer => DataType.Pointer,

                        OperandKind.ShortLabel => DataType.Byte,
                        OperandKind.LongLabel => DataType.Word,

                        OperandKind.RAX => DataType.QuadWord,
                        OperandKind.EAX => DataType.DoubleWord,
                        OperandKind.AX => DataType.Word,
                        OperandKind.AL => DataType.Byte,
                        OperandKind.AH => DataType.Byte,

                        OperandKind.RBX => DataType.QuadWord,
                        OperandKind.EBX => DataType.DoubleWord,
                        OperandKind.BX => DataType.Word,
                        OperandKind.BL => DataType.Byte,
                        OperandKind.BH => DataType.Byte,

                        OperandKind.RCX => DataType.QuadWord,
                        OperandKind.ECX => DataType.DoubleWord,
                        OperandKind.CX => DataType.Word,
                        OperandKind.CL => DataType.Byte,
                        OperandKind.CH => DataType.Byte,

                        OperandKind.RDX => DataType.QuadWord,
                        OperandKind.EDX => DataType.DoubleWord,
                        OperandKind.DX => DataType.Word,
                        OperandKind.DL => DataType.Byte,
                        OperandKind.DH => DataType.Byte,

                        OperandKind.RSP => DataType.QuadWord,
                        OperandKind.ESP => DataType.DoubleWord,
                        OperandKind.SP => DataType.Word,

                        OperandKind.RBP => DataType.QuadWord,
                        OperandKind.EBP => DataType.DoubleWord,
                        OperandKind.BP => DataType.Word,

                        OperandKind.RSI => DataType.QuadWord,
                        OperandKind.ESI => DataType.DoubleWord,
                        OperandKind.SI => DataType.Word,

                        OperandKind.RDI => DataType.QuadWord,
                        OperandKind.EDI => DataType.DoubleWord,
                        OperandKind.DI => DataType.Word,

                        OperandKind.CS or
                        OperandKind.DS or
                        OperandKind.SS or
                        OperandKind.ES or
                        OperandKind.CR => DataType.Word,

                        OperandKind.DR or
                        OperandKind.TR or
                        OperandKind.FS or
                        OperandKind.GS => DataType.Word,

                        _ => DataType.None,
                    };
                    if (inferredType > bestInferredType)
                        bestInferredType = inferredType;
                }
            }
            else
                bestInferredType = DataWidthToDataType(entry.DataWidth);

            bool isDest = true;
            foreach (Operand sourceOp in entry.Operands)
            {
                Debug.Assert(opCount < targetOps.Length);

                DataType explicitType = DataType.None;

                // Operands to skip or to validate
                switch (sourceOp.Kind)
                {
                    case OperandKind.KeywordFar:
                        Contract.Assert(entry.Flags.HasFlag(InstructionFlags.Far));
                        explicitType |= bestInferredType;
                        continue;

                    case OperandKind.TypePointer:
                        Contract.Assert(entry.DataType.HasFlag(DataType.Pointer));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandKind.TypeDoubleWord:
                        Contract.Assert(entry.DataType.HasFlag(DataType.DoubleWord));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandKind.TypeInt:
                        Contract.Assert(entry.DataType.HasFlag(DataType.Int));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandKind.TypeShort:
                        Contract.Assert(entry.DataType.HasFlag(DataType.Word));
                        explicitType |= entry.DataType;
                        continue;

                    case OperandKind.NearPointer:
                        Contract.Assert(!entry.Flags.HasFlag(InstructionFlags.Far) && entry.DataType.HasFlag(DataType.Pointer));
                        explicitType |= entry.DataType;
                        break; // We want to create an operand for this

                    case OperandKind.FarPointer:
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
                        case OperandKind.RegisterByte:
                        case OperandKind.RegisterWord:
                        case OperandKind.RegisterDoubleWord:
                        case OperandKind.RegisterOrMemoryByte:
                        case OperandKind.RegisterOrMemoryWord:
                        case OperandKind.RegisterOrMemoryDoubleWord:
                        case OperandKind.RegisterOrMemoryQuadWord:
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

                InstructionOperand targetOp = CreateOperand(sourceOp, mode, register, eac, displacement, immediate, offset, segment, sourceType);

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

        public OneOf<AssemblyLine[], Error> GetAssemblyLines(IEnumerable<Instruction> instructions, OutputValueMode outputMode, string hexPrefix = "0x")
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
                switch (instruction.Mnemonic.Type)
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
                            if (firstOp.Op == OperandType.Immediate)
                            {
                                if (!firstOp.Immediate.Flags.HasFlag(ImmediateFlag.RelativeJumpDisplacement))
                                    return new Error(ErrorCode.UnsupportedImmediateFlags, $"The immediate '{firstOp.Immediate}' is not a relative jump displacement for jump instruction '{instruction}'", instruction.Position);
                                int relativeAddressAfterThisInstruction = firstOp.Immediate.Value;
                                absoluteAddress = (uint)(instruction.Position + instruction.Length + relativeAddressAfterThisInstruction);
                            }
                            else if (firstOp.Op == OperandType.Value)
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

                    if (hasLock && instruction.Mnemonic.Type == InstructionType.XCHG)
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

        public OneOf<int, Error> ExecuteInstruction(Instruction instruction)
        {
            if (instruction == null)
                return new Error(ErrorCode.MissingInstructionParameter, $"The instruction parameter is missing!", 0);
            return _executer.Execute(instruction);
        }

        public OneOf<uint, Error> BeginStep()
        {
            if (ActiveProgram == null)
                return new Error(ErrorCode.ProgramNotLoaded, $"No program was loaded", 0);
            
            if (!(ExecutionState == ExecutionState.Stopped || ExecutionState == ExecutionState.Finished || ExecutionState == ExecutionState.Failed))
                return new Error(ErrorCode.InvalidExecutionState, $"The execution state '{ExecutionState}' is not valid for {nameof(BeginStep)}", 0);

            ExecutionState = ExecutionState.Halted;

            if (ActiveProgram.Register != null)
                Register.Assign(ActiveProgram.Register);
            else
                Register.Reset();

            CurrentIP = Register.IP;
            CurrentInstruction = null;
            PreviousIP = CurrentIP;

            return CurrentIP;
        }

        public OneOf<Instruction, Error> Step()
        {
            PreviousIP = CurrentIP;

            if (ActiveProgram == null)
                return new Error(ErrorCode.ProgramNotLoaded, $"No program was loaded", 0);

            if (ExecutionState != ExecutionState.Halted)
                return new Error(ErrorCode.InvalidExecutionState, $"The execution state '{ExecutionState}' is not valid for {nameof(Step)}", CurrentIP);

            Contract.Assert(CurrentIP < ActiveProgram.Length);
            ReadOnlySpan<byte> stream = ActiveProgram.Stream.AsSpan().Slice((int)CurrentIP);

            ExecutionState = ExecutionState.Running;

            Thread.Sleep(50);
            OneOf<Instruction, Error> decodeRes = TryDecodeNext(stream, ActiveProgram.Name, CurrentIP);
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

            Thread.Sleep(100);
            OneOf<int, Error> executionRes = ExecuteInstruction(instruction);
            if (executionRes.IsT1)
            {
                ExecutionState = ExecutionState.Failed;
                return executionRes.AsT1;
            }

            uint newIp = (ushort)(CurrentIP + executionRes.AsT0);
            CurrentIP = newIp;
            Contract.Assert(CurrentIP < ushort.MaxValue);
            Register.IP = (ushort)CurrentIP;

            if (CurrentIP == (uint)ActiveProgram.Length)
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

            CurrentIP = Register.IP;
            CurrentInstruction = null;
            PreviousIP = CurrentIP;

            while ((CurrentIP != (uint)ActiveProgram.Length) && !state.IsStopped)
            {
                uint ip = PreviousIP = CurrentIP;
                Contract.Assert(ip < ActiveProgram.Length);
                ReadOnlySpan<byte> stream = ActiveProgram.Stream.AsSpan().Slice((int)ip);

                Thread.Sleep(50);
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

                Thread.Sleep(100);
                OneOf<int, Error> executionRes = ExecuteInstruction(instruction);
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
