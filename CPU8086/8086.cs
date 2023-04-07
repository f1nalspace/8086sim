using OneOf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Final.CPU8086
{
    public class CPU : INotifyPropertyChanged
    {
        private const int MaxInstructionLength = 6;

        private readonly InstructionTable _entryTable = new InstructionTable();
        private static readonly RegisterTable _regTable = new RegisterTable();
        private static readonly EffectiveAddressCalculationTable _effectiveAddressCalculationTable = new EffectiveAddressCalculationTable();

        public event PropertyChangedEventHandler PropertyChanged;

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

        public CPURegister Register { get => _register; private set => SetValue(ref _register, value); }
        private CPURegister _register = new CPURegister();

        public CPU()
        {
            _entryTable.Load();
        }

        public void Reset()
        {
            Register = new CPURegister();
        }

        private static OneOf<byte, Error> ReadU8(ref ReadOnlySpan<byte> stream, string streamName, int position)
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

        public Instruction DecodeNext(ReadOnlySpan<byte> stream, string streamName, int position = 0)
        {
            OneOf<Instruction, Error> r = TryDecodeNext(stream, streamName, position);
            if (r.IsT0)
                return r.AsT0;
            return null;
        }

        static InstructionOperand CreateOperand(Operand sourceOp, Mode mode, byte registerBits, EffectiveAddressCalculation eac, int displacement, int immediate, DataType explicitType)
        {
            switch (sourceOp.Kind)
            {
                case OperandKind.Value:
                    return new InstructionOperand(s32: sourceOp.Value);

                case OperandKind.MemoryByte:
                case OperandKind.MemoryWord:
                case OperandKind.MemoryDoubleWord:
                case OperandKind.MemoryQuadWord:
                    return new InstructionOperand(new MemoryAddress(eac, displacement));

                case OperandKind.MemoryWordReal:
                case OperandKind.MemoryDoubleWordReal:
                case OperandKind.MemoryQuadWordReal:
                case OperandKind.MemoryTenByteReal:
                    break;

                case OperandKind.RegisterByte:
                    return new InstructionOperand(_regTable.GetByte(registerBits));
                case OperandKind.RegisterWord:
                    return new InstructionOperand(_regTable.GetWord(registerBits));
                case OperandKind.RegisterDoubleWord:
                    break;

                case OperandKind.RegisterOrMemoryByte:
                    if (mode == Mode.RegisterMode)
                        return new InstructionOperand(_regTable.GetByte(registerBits));
                    else
                        return new InstructionOperand(eac, displacement);
                case OperandKind.RegisterOrMemoryWord:
                    if (mode == Mode.RegisterMode)
                        return new InstructionOperand(_regTable.GetWord(registerBits));
                    else
                        return new InstructionOperand(eac, displacement);
                case OperandKind.RegisterOrMemoryDoubleWord:
                case OperandKind.RegisterOrMemoryQuadWord:
                    break;
                case OperandKind.ImmediateByte:
                    if ((sbyte)immediate < 0)
                        return new InstructionOperand((sbyte)(immediate & 0xFF), ImmediateFlag.None, explicitType);
                    else
                        return new InstructionOperand((byte)(immediate & 0xFF), ImmediateFlag.None, explicitType);
                case OperandKind.ImmediateWord:
                    if ((short)immediate < 0)
                        return new InstructionOperand((short)(immediate & 0xFFFF), ImmediateFlag.None, explicitType);
                    else
                        return new InstructionOperand((ushort)(immediate & 0xFFFF), ImmediateFlag.None, explicitType);
                case OperandKind.ImmediateDoubleWord:
                    if ((int)immediate < 0)
                        return new InstructionOperand((int)(immediate & 0xFFFFFFFF), ImmediateFlag.None, explicitType);
                    else
                        return new InstructionOperand((uint)(immediate & 0xFFFFFFFF), ImmediateFlag.None, explicitType);
                case OperandKind.KeywordFar:
                    break;
                case OperandKind.KeywordPointer:
                    break;

                case OperandKind.TypeDoubleWord:
                    break;
                case OperandKind.TypeShort:
                    break;
                case OperandKind.TypeInt:
                    break;

                case OperandKind.NearPointer:
                    break;
                case OperandKind.FarPointer:
                    break;
                case OperandKind.SourceRegister:
                    break;

                case OperandKind.ShortLabel:
                    if ((sbyte)immediate < 0)
                        return new InstructionOperand((sbyte)(immediate & 0xFF), ImmediateFlag.RelativeJumpDisplacement, explicitType);
                    else
                        return new InstructionOperand((byte)(immediate & 0xFF), ImmediateFlag.RelativeJumpDisplacement, explicitType);
                case OperandKind.LongLabel:
                    if ((short)immediate < 0)
                        return new InstructionOperand((short)(immediate & 0xFFFF), ImmediateFlag.RelativeJumpDisplacement, explicitType);
                    else
                        return new InstructionOperand((ushort)(immediate & 0xFFFF), ImmediateFlag.RelativeJumpDisplacement, explicitType);
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
                case OperandKind.DR:
                case OperandKind.TR:
                case OperandKind.FS:
                case OperandKind.GS:
                    break;
                default:
                    return new InstructionOperand();
            }

            throw new NotSupportedException($"The source operand '{sourceOp}' is not supported");
        }

        static OneOf<Instruction, Error> LoadInstruction(ReadOnlySpan<byte> stream, string streamName, int position, InstructionEntry entry)
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

            ReadOnlySpan<byte> cur = stream.Slice(1); // Skip op-code

            bool destinationIsRegister = (opCode & 0b00000010) == 0b00000010;

            byte modField = byte.MaxValue;
            byte regField = byte.MaxValue;
            byte rmField = byte.MaxValue;

            int displacement = 0;
            int immediate = 0;
            Mode mode = Mode.Unknown;
            EffectiveAddressCalculation eac = EffectiveAddressCalculation.None;
            int displacementLength = 0;
            bool useExplicitType = false;
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
                            mode = (Mode)modField;
                            eac = mode switch
                            {
                                Mode.RegisterMode => EffectiveAddressCalculation.None,
                                _ => _effectiveAddressCalculationTable.Get(rmField, modField)
                            };
                            if (mode != Mode.RegisterMode)
                                displacementLength = _effectiveAddressCalculationTable.GetDisplacementLength(eac);
                            else
                                displacementLength = 0;
                            useExplicitType = eac switch
                            {
                                EffectiveAddressCalculation.BX_SI or
                                EffectiveAddressCalculation.BX_DI or
                                EffectiveAddressCalculation.BP_SI or
                                EffectiveAddressCalculation.BP_DI or
                                EffectiveAddressCalculation.SI or
                                EffectiveAddressCalculation.DI or
                                EffectiveAddressCalculation.BX or
                                EffectiveAddressCalculation.BX_SI_D8 or
                                EffectiveAddressCalculation.BX_DI_D8 or
                                EffectiveAddressCalculation.BP_SI_D8 or
                                EffectiveAddressCalculation.BP_DI_D8 or
                                EffectiveAddressCalculation.SI_D8 or
                                EffectiveAddressCalculation.DI_D8 or
                                EffectiveAddressCalculation.BP_D8 or
                                EffectiveAddressCalculation.BX_D8 or
                                EffectiveAddressCalculation.BX_SI_D16 or
                                EffectiveAddressCalculation.BX_DI_D16 or
                                EffectiveAddressCalculation.BP_SI_D16 or
                                EffectiveAddressCalculation.BP_DI_D16 or
                                EffectiveAddressCalculation.SI_D16 or
                                EffectiveAddressCalculation.DI_D16 or
                                EffectiveAddressCalculation.BP_D16 or
                                EffectiveAddressCalculation.BX_D16 => true,
                                _ => false,
                            };
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
                            immediate |= ((int)d8 << shift);
                        }
                        break;

                    case FieldType.Immediate0to3:
                        {
                            immediate = 0;
                            for (int t = 0; t < 3; t++)
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

#if false
                        case FieldType.Offset0:
                        case FieldType.Offset1:
                            break;
                        case FieldType.Segment0:
                        case FieldType.Segment1:
                            break;
                        case FieldType.RelativeLabelDisplacement0:
                        case FieldType.RelativeLabelDisplacement1:
                            break;
                        case FieldType.ShortLabelOrShortLow:
                        case FieldType.LongLabel:
                        case FieldType.ShortHigh:
                            break;
#endif
                    default:
                        throw new NotSupportedException($"The field type '{field.Type}' is not supported!");
                }
            }

            Span<InstructionOperand> targetOps = stackalloc InstructionOperand[4];
            int opCount = 0;

            if (modField == byte.MaxValue)
                eac = EffectiveAddressCalculation.DirectAddress;

            bool isDest = true;
            foreach (Operand sourceOp in entry.Operands)
            {
                Debug.Assert(opCount < targetOps.Length);

                byte register = 0;
                if (mode == Mode.RegisterMode)
                {
                    if (isDest || !hasRegField)
                        register = rmField;
                    else
                        register = regField;
                }
                else if (mode != Mode.Unknown)
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

                DataType explicitType = DataType.None;
                if (useExplicitType)
                {
                    if (entry.DataWidth.Type == DataWidthType.Byte)
                        explicitType = DataType.Byte;
                    else if (entry.DataWidth.Type == DataWidthType.Word)
                        explicitType = DataType.Word;
                }

                InstructionOperand targetOp = CreateOperand(sourceOp, mode, register, eac, displacement, immediate, explicitType);

                targetOps[opCount++] = targetOp;

                if (isDest)
                    isDest = false;
            }

            int length = stream.Length - cur.Length;
            if (length > MaxInstructionLength)
                return new Error(ErrorCode.InstructionLengthTooLarge, $"The instruction length '{length}' of '{entry}' exceeds the max length of '{MaxInstructionLength}' bytes", position);
            if (length < entry.MinLength)
                return new Error(ErrorCode.InstructionLengthTooSmall, $"Expect instruction length to be at least '{entry.MinLength}' bytes, but got '{length}' bytes for instruction '{entry}'", position);
            if (length > entry.MaxLength)
                return new Error(ErrorCode.InstructionLengthTooLarge, $"The instruction length '{length}' of '{entry}' exceeds the max length of '{entry.MaxLength}' bytes", position);

            Span<InstructionOperand> actualOps = targetOps.Slice(0, opCount);
            return new Instruction(position, entry.Op, (byte)length, entry.Type, entry.DataWidth, actualOps);
        }

        public OneOf<Instruction, Error> TryDecodeNext(ReadOnlySpan<byte> stream, string streamName, int position = 0)
        {
            ReadOnlySpan<byte> tmp = stream;
            OneOf<byte, Error> opCodeRes = ReadU8(ref tmp, streamName, position);
            if (opCodeRes.IsT1)
                return opCodeRes.AsT1;

            byte opCode = opCodeRes.AsT0;
            InstructionList instructionList = _entryTable[opCode];
            if (instructionList == null)
                return new Error(ErrorCode.OpCodeNotImplemented, $"Not implemented opcode '${opCode:X2}' / '{opCode.ToBinary()}'!", position);
            else if ((byte)instructionList.Op != opCode)
                return new Error(ErrorCode.OpCodeMismatch, $"Mismatch opcode! Expect '${opCode:X2}', but got '{instructionList.Op}'", position);

            if (instructionList.Count == 1)
            {
                InstructionEntry entry = instructionList.First();
                OneOf<Instruction, Error> loadRes = LoadInstruction(stream, streamName, position, entry);
                if (loadRes.TryPickT1(out Error error, out _))
                    return new Error(error, $"Failed to decode instruction stream '{streamName}' for single entry '{entry}'", position);
                return loadRes.AsT0;
            }
            else
            {
                // Expect that there is at least one more byte
                foreach (InstructionEntry instructionEntry in instructionList)
                {
                    OneOf<Instruction, Error> loadRes = LoadInstruction(stream, streamName, position, instructionEntry);
                    if (loadRes.TryPickT0(out Instruction instruction, out _))
                        return instruction;
                }
            }

            return new Error(ErrorCode.OpCodeNotImplemented, $"No instruction entry found that matches the opcode '{opCode:X2}' in stream '{streamName}'", position);
        }

        public OneOf<string, Error> GetAssembly(ReadOnlySpan<byte> stream, string streamName, OutputValueMode outputMode, string hexPrefix = "0x")
        {
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

            ReadOnlySpan<byte> cur = stream;
            int position = 0;
            while (cur.Length > 0)
            {
                OneOf<Instruction, Error> decodeRes = TryDecodeNext(cur, streamName, position);
                if (decodeRes.IsT1)
                    return decodeRes.AsT1;

                Instruction instruction = decodeRes.AsT0;
                string asm = instruction.Asm(outputMode, hexPrefix);
                Debug.WriteLine($"\t{asm}");
                s.AppendLine(asm);

                cur = cur.Slice(instruction.Length);
                position += instruction.Length;
            }
            return s.ToString();
        }
    }
}
