using OneOf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Final.CPU8086
{
    public class CPU
    {
        private readonly InstructionEntryTable _entryTable = new InstructionEntryTable();

        private static readonly InstructionTable _opTable = new InstructionTable();
        private static readonly RegisterTable _regTable = new RegisterTable();
        private static readonly EffectiveAddressCalculationTable _effectiveAddressCalculationTable = new EffectiveAddressCalculationTable();

        public CPU()
        {
            _entryTable.Load();
        }

        private static OneOf<byte, Error> ReadU8(ref ReadOnlySpan<byte> stream, string streamName)
        {
            if (stream.Length < 1)
                return new Error(ErrorCode.NotEnoughBytesInStream, $"Cannot read U8, because stream '{streamName}' is already finished or is not long enough for 1 byte");
            byte result = stream[0];
            stream = stream.Slice(1);
            return result;
        }

        private static OneOf<sbyte, Error> ReadS8(ref ReadOnlySpan<byte> stream, string streamName)
        {
            if (stream.Length < 1)
                return new Error(ErrorCode.NotEnoughBytesInStream, $"Cannot read S8, because stream '{streamName}' is already finished or is not long enough for 1 byte");
            sbyte result = (sbyte)stream[0];
            stream = stream.Slice(1);
            return result;
        }

        private static OneOf<short, Error> ReadS16(ref ReadOnlySpan<byte> stream, string streamName)
        {
            if (stream.Length < 2)
                return new Error(ErrorCode.NotEnoughBytesInStream, $"Cannot read S16, because stream '{streamName}' is already finished or is not long enough for 2 bytes");
            byte first = stream[0];
            byte second = stream[1];
            short result = (short)(first | second << 8);
            stream = stream.Slice(2);
            return result;
        }

        public static string GetAddressAssembly(byte address8, OutputValueMode outputMode)
        {
            short displacement;
            if ((address8 & 0b10000000) == 0b10000000)
                displacement = (sbyte)address8;
            else
                displacement = address8;
            return GetValueAssembly(displacement, outputMode);
        }

        public static string GetAddressAssembly(EffectiveAddressCalculation eac, short displacementOrAddress, OutputValueMode outputMode)
        {
            string append;
            if (displacementOrAddress == 0)
                append = string.Empty;
            else
            {
                byte displacementLength = _effectiveAddressCalculationTable.GetDisplacementLength(eac);

                char op = '+';
                if (displacementLength == 1)
                {
                    if ((sbyte)displacementOrAddress < 0)
                    {
                        op = '-';
                        displacementOrAddress = Math.Abs((sbyte)displacementOrAddress);
                    }
                }
                else
                {
                    if (displacementOrAddress < 0)
                    {
                        op = '-';
                        displacementOrAddress = Math.Abs(displacementOrAddress);
                    }
                }
                string address = GetValueAssembly(displacementOrAddress, outputMode);
                append = $" {op} {address}";
            }

            return eac switch
            {
                EffectiveAddressCalculation.BX_SI => "[bx + si]",
                EffectiveAddressCalculation.BX_DI => "[bx + di]",
                EffectiveAddressCalculation.BP_SI => "[bp + si]",
                EffectiveAddressCalculation.BP_DI => "[bp + di]",
                EffectiveAddressCalculation.SI => "[si]",
                EffectiveAddressCalculation.DI => "[di]",
                EffectiveAddressCalculation.DirectAddress => $"[{GetValueAssembly(displacementOrAddress, outputMode)}]",
                EffectiveAddressCalculation.BX => "[bx]",
                EffectiveAddressCalculation.BX_SI_D8 => $"[bx + si{append}]",
                EffectiveAddressCalculation.BX_DI_D8 => $"[bx + di{append}]",
                EffectiveAddressCalculation.BP_SI_D8 => $"[bp + si{append}]",
                EffectiveAddressCalculation.BP_DI_D8 => $"[bp + di{append}]",
                EffectiveAddressCalculation.SI_D8 => $"[si{append}]",
                EffectiveAddressCalculation.DI_D8 => $"[di{append}]",
                EffectiveAddressCalculation.BP_D8 => $"[bp{append}]",
                EffectiveAddressCalculation.BX_D8 => $"[bx{append}]",
                EffectiveAddressCalculation.BX_SI_D16 => $"[bx + si{append}]",
                EffectiveAddressCalculation.BX_DI_D16 => $"[bx + di{append}]",
                EffectiveAddressCalculation.BP_SI_D16 => $"[bp + si{append}]",
                EffectiveAddressCalculation.BP_DI_D16 => $"[bp + di{append}]",
                EffectiveAddressCalculation.SI_D16 => $"[si{append}]",
                EffectiveAddressCalculation.DI_D16 => $"[di{append}]",
                EffectiveAddressCalculation.BP_D16 => $"[bp{append}]",
                EffectiveAddressCalculation.BX_D16 => $"[bx{append}]",
                _ => throw new NotImplementedException($"Not supported effective address calculation of '{eac}'"),
            };
        }

        public static string GetValueAssembly(byte value, OutputValueMode outputMode)
        {
            short v = (value & 0b10000000) == 0b10000000 ? (sbyte)value : value;
            return outputMode switch
            {
                OutputValueMode.AsHexAuto => $"0x{v:x}",
                OutputValueMode.AsHex8 => $"0x{v:x2}",
                OutputValueMode.AsHex16 => $"0x{v:x4}",
                _ => v.ToString(),
            };
        }

        public static string GetValueAssembly(short value, OutputValueMode outputMode) => outputMode switch
        {
            OutputValueMode.AsHexAuto => $"0x{value:x}",
            OutputValueMode.AsHex8 => $"0x{value:x2}",
            OutputValueMode.AsHex16 => $"0x{value:x4}",
            _ => value.ToString(),
        };

        public static string GetRegisterAssembly(RegisterType regType) => Register.GetLowerName(regType);

        public static string GetRegisterAssembly(Register reg) => GetRegisterAssembly(reg?.Type ?? RegisterType.Unknown);

        public static OneOf<string, Error> GetAssembly(Stream stream, string name, OutputValueMode outputMode)
        {
            long len = stream.Length;
            byte[] data = new byte[len];
            stream.Read(data);
            return GetAssembly(data, name, outputMode);
        }

        readonly struct ModRegRM
        {
            public byte ModField { get; }
            public byte RegField { get; }
            public byte RMField { get; }

            public Mode Mode { get; }
            public EffectiveAddressCalculation EAC { get; }

            public ModRegRM(byte modField, byte regField, byte rmField)
            {
                ModField = modField;
                RegField = regField;
                RMField = rmField;
                Mode = (Mode)modField;
                EAC = Mode switch
                {
                    Mode.RegisterMode => EffectiveAddressCalculation.None,
                    _ => _effectiveAddressCalculationTable.Get(rmField, modField)
                };
            }
        }

        private static ModRegRM ReadModRegRM(byte value)
        {
            byte modField = (byte)(value >> 6 & 0b111);
            byte regField = (byte)(value >> 3 & 0b111);
            byte rmField = (byte)(value >> 0 & 0b111);
            ModRegRM result = new ModRegRM(modField, regField, rmField);
            return result;
        }

        private static OneOf<short, Error> LoadDisplacementOrZero(EffectiveAddressCalculation eac, ref ReadOnlySpan<byte> stream, string streamName, out byte outLen)
        {
            outLen = eac switch
            {
                // Mod == 01
                EffectiveAddressCalculation.BX_SI_D8 or
                EffectiveAddressCalculation.BX_DI_D8 or
                EffectiveAddressCalculation.BP_SI_D8 or
                EffectiveAddressCalculation.BP_DI_D8 or
                EffectiveAddressCalculation.SI_D8 or
                EffectiveAddressCalculation.DI_D8 or
                EffectiveAddressCalculation.BP_D8 or
                EffectiveAddressCalculation.BX_D8 => 1,

                // Mod == 10
                EffectiveAddressCalculation.DirectAddress or
                EffectiveAddressCalculation.BX_SI_D16 or
                EffectiveAddressCalculation.BX_DI_D16 or
                EffectiveAddressCalculation.BP_SI_D16 or
                EffectiveAddressCalculation.BP_DI_D16 or
                EffectiveAddressCalculation.SI_D16 or
                EffectiveAddressCalculation.DI_D16 or
                EffectiveAddressCalculation.BP_D16 or
                EffectiveAddressCalculation.BX_D16 => 2,

                _ => 0,
            };

            if (outLen == 1)
            {
                OneOf<byte, Error> u8 = ReadU8(ref stream, streamName);
                if (u8.IsT1)
                    return new Error(u8.AsT1, $"Cannot load 8-bit displacement for EAC '{eac}'");
                return u8.AsT0;
            }
            else if (outLen == 2)
            {
                OneOf<short, Error> s16 = ReadS16(ref stream, streamName);
                if (s16.IsT1)
                    return new Error(s16.AsT1, $"Cannot load 16-bit displacement for EAC '{eac}'");
                return s16.AsT0;
            }
            else
                return 0;
        }

        private static (string destination, string source) GetDestinationAndSource(ModRegRM modRegRM, bool destinationIsToRegister, bool isWord, short displacement, OutputValueMode outputMode)
        {
            string destination, source;
            if (modRegRM.Mode == Mode.RegisterMode)
            {
                if (isWord)
                {
                    // 16-bit Register to Register
                    if (destinationIsToRegister)
                    {
                        destination = GetRegisterAssembly(_regTable.GetWord(modRegRM.RegField));
                        source = GetRegisterAssembly(_regTable.GetWord(modRegRM.RMField));
                    }
                    else
                    {
                        destination = GetRegisterAssembly(_regTable.GetWord(modRegRM.RMField));
                        source = GetRegisterAssembly(_regTable.GetWord(modRegRM.RegField));
                    }
                }
                else
                {
                    // 8-bit Register to Register
                    if (destinationIsToRegister)
                    {
                        destination = GetRegisterAssembly(_regTable.GetByte(modRegRM.RegField));
                        source = GetRegisterAssembly(_regTable.GetByte(modRegRM.RMField));
                    }
                    else
                    {
                        destination = GetRegisterAssembly(_regTable.GetByte(modRegRM.RMField));
                        source = GetRegisterAssembly(_regTable.GetByte(modRegRM.RegField));
                    }
                }
            }
            else
            {
                if (isWord)
                {
                    if (destinationIsToRegister)
                    {
                        // 16-bit Memory to Register
                        destination = GetRegisterAssembly(_regTable.GetWord(modRegRM.RegField));
                        source = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                    }
                    else
                    {
                        // 16-bit Register to Memory
                        destination = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                        source = GetRegisterAssembly(_regTable.GetWord(modRegRM.RegField));
                    }
                }
                else
                {
                    if (destinationIsToRegister)
                    {
                        // 8-bit Memory to Register
                        destination = GetRegisterAssembly(_regTable.GetByte(modRegRM.RegField));
                        source = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                    }
                    else
                    {
                        // 8-bit Register to Memory
                        destination = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                        source = GetRegisterAssembly(_regTable.GetByte(modRegRM.RegField));
                    }
                }
            }
            return (destination, source);
        }



        private static (InstructionOperand Dest, InstructionOperand Source) GetDestinationAndSource(ModRegRM modRegRM, bool destinationIsToRegister, bool isWord, short displacement)
        {
            if (modRegRM.Mode == Mode.RegisterMode)
            {
                if (isWord)
                {
                    // 16-bit Register to Register
                    if (destinationIsToRegister)
                    {
                        return (
                            new InstructionOperand(_regTable.GetWord(modRegRM.RegField)),
                            new InstructionOperand(_regTable.GetWord(modRegRM.RMField))
                        );
                    }
                    else
                    {
                        return (
                            new InstructionOperand(_regTable.GetWord(modRegRM.RMField)),
                            new InstructionOperand(_regTable.GetWord(modRegRM.RegField))
                        );
                    }
                }
                else
                {
                    // 8-bit Register to Register
                    if (destinationIsToRegister)
                    {
                        return (
                            new InstructionOperand(_regTable.GetByte(modRegRM.RegField)),
                            new InstructionOperand(_regTable.GetByte(modRegRM.RMField))
                        );
                    }
                    else
                    {
                        return (
                            new InstructionOperand(_regTable.GetByte(modRegRM.RMField)),
                            new InstructionOperand(_regTable.GetByte(modRegRM.RegField))
                        );
                    }
                }
            }
            else
            {
                if (isWord)
                {
                    if (destinationIsToRegister)
                    {
                        // 16-bit Memory to Register
                        return (
                            new InstructionOperand(_regTable.GetWord(modRegRM.RegField)),
                            new InstructionOperand(modRegRM.EAC, displacement)
                        );
                    }
                    else
                    {
                        // 16-bit Register to Memory
                        return (
                            new InstructionOperand(modRegRM.EAC, displacement),
                            new InstructionOperand(_regTable.GetWord(modRegRM.RegField))
                        );
                    }
                }
                else
                {
                    if (destinationIsToRegister)
                    {
                        // 8-bit Memory to Register
                        return (
                            new InstructionOperand(_regTable.GetByte(modRegRM.RegField)),
                            new InstructionOperand(modRegRM.EAC, displacement)
                        );
                    }
                    else
                    {
                        // 8-bit Register to Memory
                        return (
                            new InstructionOperand(modRegRM.EAC, displacement),
                            new InstructionOperand(_regTable.GetByte(modRegRM.RegField))
                        );
                    }
                }
            }
        }

        public Instruction DecodeNext(ReadOnlySpan<byte> stream, string streamName)
        {
            var r = TryDecodeNext(stream, streamName);
            if (r.IsT0)
                return r.AsT0;
            return new Instruction();
        }

        public OneOf<Instruction, Error> TryDecodeNext(ReadOnlySpan<byte> stream, string streamName)
        {
            byte length = 0;

            OneOf<byte, Error> opCodeRes = ReadU8(ref stream, streamName);
            if (opCodeRes.IsT1)
                return opCodeRes.AsT1;
            ++length;

            byte opCode = opCodeRes.AsT0;
            InstructionList instructionList = _entryTable[opCode];
            if (instructionList == null)
                return new Error(ErrorCode.OpCodeNotImplemented, $"Not implemented opcode '${opCode:X2}' / '{opCode.ToBinary()}'!");
            else if ((byte)instructionList.Op != opCode)
                return new Error(ErrorCode.OpCodeMismatch, $"Mismatch opcode! Expect '${opCode:X2}', but got '{instructionList.Op}'");

            bool LoadInstruction(ref ReadOnlySpan<byte> cur, InstructionEntry entry, out Instruction result)
            {
                ReadOnlySpan<byte> initial = cur;

                result = new Instruction();

                byte modField = byte.MaxValue;
                byte regField = byte.MaxValue;
                byte rmField = byte.MaxValue;

                int displacement = 0;
                int immediate = 0;
                Mode mode = Mode.Unknown;
                EffectiveAddressCalculation eac = EffectiveAddressCalculation.None;

                foreach (Field field in entry.Fields)
                {
                    switch (field.Type)
                    {
                        case FieldType.Constant:
                            {
                                OneOf<byte, Error> value = ReadU8(ref cur, streamName);
                                if (value.IsT1)
                                    return false;
                                if (field.Value != value.AsT0)
                                    return false;
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
                                OneOf<byte, Error> value = ReadU8(ref cur, streamName);
                                if (value.IsT1)
                                    return false;
                                byte mrm = value.AsT0;
                                modField = (byte)(mrm >> 6 & 0b111);
                                regField = (byte)(mrm >> 3 & 0b111);
                                rmField = (byte)(mrm >> 0 & 0b111);
                                if (field.Type != FieldType.ModRegRM)
                                {
                                    byte expectReg = field.Type - FieldType.Mod000RM;
                                    if (expectReg != regField)
                                        return false;
                                }
                                mode = (Mode)modField;
                                eac = mode switch
                                {
                                    Mode.RegisterMode => EffectiveAddressCalculation.None,
                                    _ => _effectiveAddressCalculationTable.Get(rmField, modField)
                                };
                            }
                            break;
                        case FieldType.Displacement0:
                        case FieldType.Displacement1:
                            {
                                if (eac != EffectiveAddressCalculation.None && mode != Mode.RegisterMode)
                                {
                                    OneOf<byte, Error> value = ReadU8(ref cur, streamName);
                                    if (value.IsT1)
                                        return false;
                                    byte d8 = value.AsT0;
                                    int shift = 1 >> ((field.Type - FieldType.Displacement0) * 2);
                                    displacement |= (d8 << shift);
                                }
                            }
                            break;
                        case FieldType.Immediate0:
                        case FieldType.Immediate1:
                        case FieldType.Immediate2:
                        case FieldType.Immediate3:
                            {
                                OneOf<byte, Error> value = ReadU8(ref cur, streamName);
                                if (value.IsT1)
                                    return false;
                                byte imm8 = value.AsT0;

                                int t = (int)field.Type - (int)FieldType.Immediate0;
                                int shift = t * 8;
                                immediate |= ((int)imm8 << shift);
                            }
                            break;
                        case FieldType.Immediate0to3:
                            break;
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
                        default:
                            break;
                    }
                }

                Span<InstructionOperand> targetOps = stackalloc InstructionOperand[4];
                int opCount = 0;

                static InstructionOperand CreateOperand(Operand sourceOp, Mode mode, byte registerBits, EffectiveAddressCalculation eac, int displacement, int immediate)
                {
                    switch (sourceOp.Kind)
                    {
                        case OperandKind.Value:
                            return new InstructionOperand(sourceOp.Value);

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
                            return new InstructionOperand((byte)immediate, ImmediateFlag.None);
                        case OperandKind.ImmediateWord:
                            return new InstructionOperand((short)immediate, ImmediateFlag.None);
                        case OperandKind.ImmediateDoubleWord:
                            break;

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
                            break;
                        case OperandKind.LongLabel:
                            break;
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

                    throw new NotSupportedException($"The operand type '{sourceOp}' is not supported");
                }

                bool isDest = true;
                foreach (Operand sourceOp in entry.Operands)
                {
                    Debug.Assert(opCount < targetOps.Length);

                    byte registerBits = isDest ? rmField : regField;
                    if (isDest)
                        isDest = false;

                    InstructionOperand targetOp = CreateOperand(sourceOp, mode, registerBits, eac, displacement, immediate);

                    targetOps[opCount++] = targetOp;
                }

                int length = 1 + (initial.Length - cur.Length); // + 1 for including the opcode, because fields are without the opcode
                if (length < entry.MinLength || length > entry.MaxLength || length > 6)
                    return false;

                result = new Instruction(entry.Op, (byte)length, entry.Type, entry.DataWidth, targetOps.Slice(0, opCount));

                return true;
            }

            if (instructionList.Count == 1)
            {
                InstructionEntry entry = instructionList.First();
                if (LoadInstruction(ref stream, entry, out Instruction instruction))
                    return instruction;
            }
            else
            {
                // Expect that there is at least one more byte
                Span<byte> preloadedFields = stackalloc byte[5];
                foreach (InstructionEntry instructionEntry in instructionList)
                {
                    ReadOnlySpan<byte> tmp = stream;
                    if (LoadInstruction(ref tmp, instructionEntry, out Instruction instruction))
                    {
                        stream = tmp;
                        return instruction;
                    }
                }
            }

            return new Error(ErrorCode.OpCodeNotImplemented, $"No instruction entry found, that matches the opcode '{opCode}'");

            //return new Instruction(opCode, length)
#if false
            static InstructionType GetInstructionType(OpFamily family)
            {
                return family switch
                {
                    OpFamily.Push_FixedReg => InstructionType.PUSH,
                    OpFamily.Pop_FixedReg => InstructionType.POP,

                    OpFamily.Move8_RegOrMem_RegOrMem or
                    OpFamily.Move16_RegOrMem_RegOrMem or
                    OpFamily.Move8_Reg_Imm or
                    OpFamily.Move16_Reg_Imm or
                    OpFamily.Move8_Mem_Imm or
                    OpFamily.Move16_Mem_Imm or
                    OpFamily.Move8_FixedReg_Mem or
                    OpFamily.Move16_FixedReg_Mem or
                    OpFamily.Move8_Mem_FixedReg or
                    OpFamily.Move16_Mem_FixedReg => InstructionType.MOV,

                    OpFamily.Add8_RegOrMem_RegOrMem or
                    OpFamily.Add16_RegOrMem_RegOrMem or
                    OpFamily.Add8_FixedReg_Imm or
                    OpFamily.Add16_FixedReg_Imm => InstructionType.ADD,

                    OpFamily.Or8_RegOrMem_RegOrMem or
                    OpFamily.Or16_RegOrMem_RegOrMem or
                    OpFamily.Or8_FixedReg_Imm or
                    OpFamily.Or16_FixedReg_Imm => InstructionType.OR,

                    OpFamily.Adc8_RegOrMem_RegOrMem or
                    OpFamily.Adc16_RegOrMem_RegOrMem or
                    OpFamily.Adc8_FixedReg_Imm or
                    OpFamily.Adc16_FixedReg_Imm => InstructionType.ADC,

                    OpFamily.Sbb8_RegOrMem_RegOrMem or
                    OpFamily.Sbb16_RegOrMem_RegOrMem or
                    OpFamily.Sbb8_FixedReg_Imm or
                    OpFamily.Sbb16_FixedReg_Imm => InstructionType.SBB,

                    OpFamily.And8_RegOrMem_RegOrMem or
                    OpFamily.And16_RegOrMem_RegOrMem or
                    OpFamily.And8_FixedReg_Imm or
                    OpFamily.And16_FixedReg_Imm => InstructionType.AND,

                    OpFamily.Sub8_RegOrMem_RegOrMem or
                    OpFamily.Sub16_RegOrMem_RegOrMem or
                    OpFamily.Sub8_FixedReg_Imm or
                    OpFamily.Sub16_FixedReg_Imm => InstructionType.SUB,

                    OpFamily.Xor8_RegOrMem_RegOrMem or
                    OpFamily.Xor16_RegOrMem_RegOrMem or
                    OpFamily.Xor8_FixedReg_Imm or
                    OpFamily.Xor16_FixedReg_Imm => InstructionType.XOR,

                    OpFamily.Cmp8_RegOrMem_RegOrMem or
                    OpFamily.Cmp16_RegOrMem_RegOrMem or
                    OpFamily.Cmp8_FixedReg_Imm or
                    OpFamily.Cmp16_FixedReg_Imm => InstructionType.CMP,

                    _ => InstructionType.None,
                };
            }

            byte length = 0;

            OneOf<byte, Error> opCodeRes = ReadU8(ref stream, streamName);
            if (opCodeRes.IsT1)
                return opCodeRes.AsT1;
            ++length;

            byte opCode = opCodeRes.AsT0;
            InstructionDefinition instruction = _opTable[opCode];
            if (instruction == null)
                return new Error(ErrorCode.OpCodeNotImplemented, $"Not implemented opcode '${opCode:X2}' / '{opCode.ToBinary()}'!");
            else if ((byte)instruction.OpCode != opCode)
                return new Error(ErrorCode.OpCodeMismatch, $"Mismatch opcode! Expect '${opCode:X2}', but got '{instruction.OpCode}'");

            // Load MOD, REG and R/M field, if needed
            ModRegRM modRegRM;
            if (instruction.Encoding.HasFlag(FieldEncoding.Mod) ||
                instruction.Encoding.HasFlag(FieldEncoding.Reg) ||
                instruction.Encoding.HasFlag(FieldEncoding.RM))
            {
                OneOf<byte, Error> u8 = ReadU8(ref stream, streamName);
                if (u8.IsT1)
                    return u8.AsT1;
                ++length;
                modRegRM = ReadModRegRM(u8.AsT0);
            }
            else
                modRegRM = new ModRegRM();

            // Load displacement, if needed
            short displacement;
            {
                OneOf<short, Error> displacementRes = LoadDisplacementOrZero(modRegRM.EAC, ref stream, streamName, out byte displacementLength);
                if (displacementRes.IsT1)
                    return displacementRes.AsT1;
                length += displacementLength;
                displacement = displacementRes.AsT0;
            }

            bool isWord = (opCode & 0b00000001) == 0b00000001;
            bool destinationIsRegister = (opCode & 0b00000010) == 0b00000010;

            // @TODO(final): Move this into Register directly
            DataWidth dataType = isWord ? DataWidthType.Word : DataWidthType.Byte;
            InstructionType type = GetInstructionType(instruction.Family);
            
            switch (instruction.Family)
            {
                case OpFamily.Push_FixedReg:
                    {
                        RegisterType register = instruction.Register;
                        Debug.Assert(register != RegisterType.Unknown);
                        return new Instruction(opCode, length, type, DataWidthType.Word, new InstructionOperand(register));
                    }

                case OpFamily.Pop_FixedReg:
                    {
                        RegisterType register = instruction.Register;
                        Debug.Assert(register != RegisterType.Unknown);
                        return new Instruction(opCode, length, type, DataWidthType.Word, new InstructionOperand(register));
                    }

                case OpFamily.Move8_RegOrMem_RegOrMem:
                case OpFamily.Move16_RegOrMem_RegOrMem:
                    {
                        // 8-bit/16-bit Move Register/Register to Register/Register
                        var ops = GetDestinationAndSource(modRegRM, destinationIsRegister, isWord, displacement);
                        return new Instruction(opCode, length, type, dataType, ops.Dest, ops.Source);
                    }

                case OpFamily.Move8_Reg_Imm:
                    {
                        // 8-bit Move Immediate To Register
                        OneOf<byte, Error> imm8 = ReadU8(ref stream, streamName);
                        if (imm8.IsT1)
                            return imm8.AsT1;
                        ++length;
                        byte reg = (byte)(opCode & 0b00000111);
                        InstructionOperand destination = new InstructionOperand(_regTable.GetByte(reg));
                        InstructionOperand source = new InstructionOperand(imm8.AsT0);
                        return new Instruction(opCode, length, type, DataWidthType.Byte, destination, source);
                    }
                case OpFamily.Move8_Mem_Imm:
                    {
                        // 8-bit Move Explicit Immediate to Memory
                        OneOf<byte, Error> imm8 = ReadU8(ref stream, streamName);
                        if (imm8.IsT1)
                            return imm8.AsT1;
                        ++length;
                        InstructionOperand destination = new InstructionOperand(modRegRM.EAC, displacement);
                        InstructionOperand source = new InstructionOperand(imm8.AsT0);
                        return new Instruction(opCode, length, type, DataWidthType.Byte, destination, source);
                    }

                case OpFamily.Move16_Reg_Imm:
                    {
                        // 16-bit Move Immediate To Register
                        OneOf<short, Error> imm16 = ReadS16(ref stream, streamName);
                        if (imm16.IsT1)
                            return imm16.AsT1;
                        length += 2;
                        byte reg = (byte)(opCode & 0b00000111);
                        InstructionOperand destination = new InstructionOperand(_regTable.GetWord(reg));
                        InstructionOperand source = new InstructionOperand(imm16.AsT0);
                        return new Instruction(opCode, length, type, DataWidthType.Word, destination, source);
                    }
                case OpFamily.Move16_Mem_Imm:
                    {
                        // 16-bit Move Explicit Immediate to Memory
                        OneOf<short, Error> imm16 = ReadS16(ref stream, streamName);
                        if (imm16.IsT1)
                            return imm16.AsT1;
                        length += 2;
                        InstructionOperand destination = new InstructionOperand(modRegRM.EAC, displacement);
                        InstructionOperand source = new InstructionOperand(imm16.AsT0);
                        return new Instruction(opCode, length, type, DataWidthType.Word, destination, source);
                    }

                case OpFamily.Move8_FixedReg_Mem:
                case OpFamily.Move16_FixedReg_Mem:
                case OpFamily.Move8_Mem_FixedReg:
                case OpFamily.Move16_Mem_FixedReg:
                    {
                        // 8-bit/16-bit Move Memory to Fixed-Register or 8-bit/16-bit Fixed-Register to Memory

                        bool destinationIsMemory = destinationIsRegister;

                        OneOf<short, Error> mem16 = ReadS16(ref stream, streamName);
                        if (mem16.IsT1)
                            return mem16.AsT1;
                        length += 2;

                        RegisterType reg = instruction.Register;
                        Debug.Assert(reg != RegisterType.Unknown);

                        (InstructionOperand Dest, InstructionOperand Source) ops;
                        if (destinationIsMemory)
                        {
                            ops = (
                                new InstructionOperand(EffectiveAddressCalculation.DirectAddress, mem16.AsT0),
                                new InstructionOperand(reg)
                            );
                        }
                        else
                        {
                            ops = (
                                new InstructionOperand(reg),
                                new InstructionOperand(EffectiveAddressCalculation.DirectAddress, mem16.AsT0)
                            );
                        }
                        return new Instruction(opCode, length, InstructionType.MOV, dataType, ops.Dest, ops.Source);
                    }

                case OpFamily.Add8_RegOrMem_RegOrMem:
                case OpFamily.Add16_RegOrMem_RegOrMem:
                case OpFamily.Or8_RegOrMem_RegOrMem:
                case OpFamily.Or16_RegOrMem_RegOrMem:
                case OpFamily.Adc8_RegOrMem_RegOrMem:
                case OpFamily.Adc16_RegOrMem_RegOrMem:
                case OpFamily.Sbb8_RegOrMem_RegOrMem:
                case OpFamily.Sbb16_RegOrMem_RegOrMem:
                case OpFamily.And8_RegOrMem_RegOrMem:
                case OpFamily.And16_RegOrMem_RegOrMem:
                case OpFamily.Sub8_RegOrMem_RegOrMem:
                case OpFamily.Sub16_RegOrMem_RegOrMem:
                case OpFamily.Xor8_RegOrMem_RegOrMem:
                case OpFamily.Xor16_RegOrMem_RegOrMem:
                case OpFamily.Cmp8_RegOrMem_RegOrMem:
                case OpFamily.Cmp16_RegOrMem_RegOrMem:
                    {
                        // 8-bit/16-bit math operation Register/Memory with Register/Memory
                        var ops = GetDestinationAndSource(modRegRM, destinationIsRegister, isWord, displacement);
                        return new Instruction(opCode, length, type, dataType, ops.Dest, ops.Source);
                    }

                case OpFamily.Add8_FixedReg_Imm:
                case OpFamily.Add16_FixedReg_Imm:
                case OpFamily.Or8_FixedReg_Imm:
                case OpFamily.Or16_FixedReg_Imm:
                case OpFamily.Adc8_FixedReg_Imm:
                case OpFamily.Adc16_FixedReg_Imm:
                case OpFamily.Sbb8_FixedReg_Imm:
                case OpFamily.Sbb16_FixedReg_Imm:
                case OpFamily.And8_FixedReg_Imm:
                case OpFamily.And16_FixedReg_Imm:
                case OpFamily.Sub8_FixedReg_Imm:
                case OpFamily.Sub16_FixedReg_Imm:
                case OpFamily.Xor8_FixedReg_Imm:
                case OpFamily.Xor16_FixedReg_Imm:
                case OpFamily.Cmp8_FixedReg_Imm:
                case OpFamily.Cmp16_FixedReg_Imm:
                    {
                        // 8-bit/16-bit Logical OR Immediate with Fixed Register
                        RegisterType reg = instruction.Register;
                        Debug.Assert(reg != RegisterType.Unknown);

                        InstructionOperand destination = new InstructionOperand(reg);

                        InstructionOperand source;
                        if (isWord)
                        {
                            OneOf<short, Error> imm16 = ReadS16(ref stream, streamName);
                            if (imm16.IsT1)
                                return imm16.AsT1;
                            length += 2;
                            source = new InstructionOperand(imm16.AsT0);
                        }
                        else
                        {
                            OneOf<byte, Error> imm8 = ReadU8(ref stream, streamName);
                            if (imm8.IsT1)
                                return imm8.AsT1;
                            length += 1;
                            source = new InstructionOperand(imm8.AsT0);
                        }
                        return new Instruction(opCode, length, type, dataType, destination, source);
                    }

                case OpFamily.Arithmetic8_RegOrMem_Imm:
                case OpFamily.Arithmetic16_RegOrMem_Imm:
                    {
                        // 8-bit/16-bit Arithmetic (ADD, SUB, OR, AND, etc.) Immediate to Register/Memory

                        bool specialCase = destinationIsRegister;

                        ArithmeticType arithmeticType = (ArithmeticType)modRegRM.RegField;

                        InstructionOperand destination, source;
                        if (isWord && specialCase || !isWord)
                        {
                            // @NOTE(final): 8-bit immediate, but 16-bit register or memory
                            OneOf<byte, Error> imm8 = ReadU8(ref stream, streamName);
                            if (imm8.IsT1)
                                return imm8.AsT1;
                            length += 1;

                            if (modRegRM.Mode == Mode.RegisterMode)
                            {
                                if (isWord)
                                    destination = new InstructionOperand(_regTable.GetWord(modRegRM.RMField));
                                else
                                    destination = new InstructionOperand(_regTable.GetByte(modRegRM.RMField));
                                source = new InstructionOperand(imm8.AsT0);
                            }
                            else
                            {
                                destination = new InstructionOperand(modRegRM.EAC, displacement);
                                source = new InstructionOperand(imm8.AsT0);
                            }
                        }
                        else
                        {
                            Debug.Assert(isWord && !specialCase);

                            // @NOTE(final): 16-bit immediate and 16-bit register or memory
                            OneOf<short, Error> imm16 = ReadS16(ref stream, streamName);
                            if (imm16.IsT1)
                                return imm16.AsT1;
                            length += 2;

                            if (modRegRM.Mode == Mode.RegisterMode)
                            {
                                if (isWord)
                                    destination = new InstructionOperand(_regTable.GetWord(modRegRM.RMField));
                                else
                                    destination = new InstructionOperand(_regTable.GetByte(modRegRM.RMField));
                                source = new InstructionOperand(imm16.AsT0);
                            }
                            else
                            {
                                destination = new InstructionOperand(modRegRM.EAC, displacement);
                                source = new InstructionOperand(imm16.AsT0);
                            }
                        }

                        type = arithmeticType switch
                        {
                            ArithmeticType.Add => InstructionType.ADD,
                            ArithmeticType.AddWithCarry => InstructionType.ADC,
                            ArithmeticType.SubWithBorrow => InstructionType.SBB,
                            ArithmeticType.Sub => InstructionType.SUB,
                            ArithmeticType.Compare => InstructionType.CMP,
                            _ => throw new NotSupportedException($"Arithmetic type '{arithmeticType}' is not supported for instruction '{instruction}'!")
                        };

                        return new Instruction(opCode, length, type, dataType, destination, source);
                    }

                default:
                    return new Error(ErrorCode.InstructionNotImplemented, $"Not implemented instruction '{instruction}'!");

            }
#endif
        }

        public static OneOf<string, Error> GetAssembly(ReadOnlySpan<byte> stream, string streamName, OutputValueMode outputMode)
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
            while (cur.Length > 0)
            {
                ReadOnlySpan<byte> start = cur;

                OneOf<byte, Error> opCodeRes = ReadU8(ref cur, streamName);
                if (opCodeRes.IsT1)
                    return opCodeRes.AsT1;

                byte opCode = opCodeRes.AsT0;

                InstructionDefinition instruction = _opTable[opCode];
                if (instruction == null)
                    return new Error(ErrorCode.OpCodeNotImplemented, $"Not implemented opcode '${opCode:X2}' / '{opCode.ToBinary()}'!");
                else if ((byte)instruction.OpCode != opCode)
                    return new Error(ErrorCode.OpCodeMismatch, $"Mismatch opcode! Expect '${opCode:X2}', but got '{instruction.OpCode}'");

                // Load MOD, REG and R/M field, if needed
                ModRegRM modRegRM;
                if (instruction.Encoding.HasFlag(FieldEncoding.ModRemRM))
                {
                    OneOf<byte, Error> u8 = ReadU8(ref cur, streamName);
                    if (u8.IsT1)
                        return u8.AsT1;
                    modRegRM = ReadModRegRM(u8.AsT0);
                }
                else
                    modRegRM = new ModRegRM();

                // Load displacement, if needed
                short displacement;
                {
                    OneOf<short, Error> displacementRes = LoadDisplacementOrZero(modRegRM.EAC, ref cur, streamName, out _);
                    if (displacementRes.IsT1)
                        return displacementRes.AsT1;
                    displacement = displacementRes.AsT0;
                }

                bool isWord = (opCode & 0b00000001) == 0b00000001;
                bool destinationIsRegister = (opCode & 0b00000010) == 0b00000010;

                AssemblyLine assemblyLine = new AssemblyLine(instruction.Mnemonic);

                switch (instruction.Family)
                {
                    case OpFamily.Push_FixedReg:
                    case OpFamily.Pop_FixedReg:
                        {
                            RegisterType register = instruction.Register;
                            Debug.Assert(register != RegisterType.Unknown);

                            // Push/Pop Fixed Register
                            string destination = GetRegisterAssembly(register);
                            assemblyLine = assemblyLine.WithDestinationOnly(destination);
                        }
                        break;

                    case OpFamily.Move8_RegOrMem_RegOrMem:
                    case OpFamily.Move16_RegOrMem_RegOrMem:
                        {
                            // 8-bit/16-bit Move Register/Register to Register/Register
                            (string destination, string source) = GetDestinationAndSource(modRegRM, destinationIsRegister, isWord, displacement, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Move8_Reg_Imm:
                        {
                            // 8-bit Move Immediate To Register
                            OneOf<byte, Error> imm8 = ReadU8(ref cur, streamName);
                            if (imm8.IsT1)
                                return imm8.AsT1;
                            byte reg = (byte)(opCode & 0b00000111);
                            string destination = GetRegisterAssembly(_regTable.GetByte(reg));
                            string source = GetValueAssembly(imm8.AsT0, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;
                    case OpFamily.Move8_Mem_Imm:
                        {
                            // 8-bit Move Explicit Immediate to Memory
                            OneOf<byte, Error> imm8 = ReadU8(ref cur, streamName);
                            if (imm8.IsT1)
                                return imm8.AsT1;
                            string destination = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                            string source = $"byte {GetValueAssembly(imm8.AsT0, outputMode)}";
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Move16_Reg_Imm:
                        {
                            // 16-bit Move Immediate To Register
                            OneOf<short, Error> imm16 = ReadS16(ref cur, streamName);
                            if (imm16.IsT1)
                                return imm16.AsT1;
                            byte reg = (byte)(opCode & 0b00000111);
                            string destination = GetRegisterAssembly(_regTable.GetWord(reg));
                            string source = GetValueAssembly(imm16.AsT0, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;
                    case OpFamily.Move16_Mem_Imm:
                        {
                            // 16-bit Move Explicit Immediate to Memory
                            OneOf<short, Error> imm16 = ReadS16(ref cur, streamName);
                            if (imm16.IsT1)
                                return imm16.AsT1;
                            string destination = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                            string source = $"word {GetValueAssembly(imm16.AsT0, outputMode)}";
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Move8_FixedReg_Mem:
                    case OpFamily.Move16_FixedReg_Mem:
                    case OpFamily.Move8_Mem_FixedReg:
                    case OpFamily.Move16_Mem_FixedReg:
                        {
                            // 8-bit/16-bit Move Memory to Fixed-Register or 8-bit/16-bit Fixed-Register to Memory

                            bool destinationIsMemory = destinationIsRegister;

                            OneOf<short, Error> mem16 = ReadS16(ref cur, streamName);
                            if (mem16.IsT1)
                                return mem16.AsT1;

                            RegisterType reg = instruction.Register;
                            Debug.Assert(reg != RegisterType.Unknown);

                            string source, destination;
                            if (destinationIsMemory)
                            {
                                destination = GetAddressAssembly(EffectiveAddressCalculation.DirectAddress, mem16.AsT0, outputMode);
                                source = GetRegisterAssembly(reg);
                            }
                            else
                            {
                                destination = GetRegisterAssembly(reg);
                                source = GetAddressAssembly(EffectiveAddressCalculation.DirectAddress, mem16.AsT0, outputMode);
                            }
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Add8_RegOrMem_RegOrMem:
                    case OpFamily.Add16_RegOrMem_RegOrMem:
                    case OpFamily.Or8_RegOrMem_RegOrMem:
                    case OpFamily.Or16_RegOrMem_RegOrMem:
                    case OpFamily.Adc8_RegOrMem_RegOrMem:
                    case OpFamily.Adc16_RegOrMem_RegOrMem:
                    case OpFamily.Sbb8_RegOrMem_RegOrMem:
                    case OpFamily.Sbb16_RegOrMem_RegOrMem:
                    case OpFamily.And8_RegOrMem_RegOrMem:
                    case OpFamily.And16_RegOrMem_RegOrMem:
                    case OpFamily.Sub8_RegOrMem_RegOrMem:
                    case OpFamily.Sub16_RegOrMem_RegOrMem:
                    case OpFamily.Xor8_RegOrMem_RegOrMem:
                    case OpFamily.Xor16_RegOrMem_RegOrMem:
                    case OpFamily.Cmp8_RegOrMem_RegOrMem:
                    case OpFamily.Cmp16_RegOrMem_RegOrMem:
                        {
                            // 8-bit/16-bit Logical OR Register/Memory with Register/Memory
                            (string destination, string source) = GetDestinationAndSource(modRegRM, destinationIsRegister, isWord, displacement, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Add8_FixedReg_Imm:
                    case OpFamily.Add16_FixedReg_Imm:
                    case OpFamily.Or8_FixedReg_Imm:
                    case OpFamily.Or16_FixedReg_Imm:
                    case OpFamily.Adc8_FixedReg_Imm:
                    case OpFamily.Adc16_FixedReg_Imm:
                    case OpFamily.Sbb8_FixedReg_Imm:
                    case OpFamily.Sbb16_FixedReg_Imm:
                    case OpFamily.And8_FixedReg_Imm:
                    case OpFamily.And16_FixedReg_Imm:
                    case OpFamily.Sub8_FixedReg_Imm:
                    case OpFamily.Sub16_FixedReg_Imm:
                    case OpFamily.Xor8_FixedReg_Imm:
                    case OpFamily.Xor16_FixedReg_Imm:
                    case OpFamily.Cmp8_FixedReg_Imm:
                    case OpFamily.Cmp16_FixedReg_Imm:
                        {
                            // 8-bit/16-bit Logical OR Immediate with Fixed Register
                            RegisterType reg = instruction.Register;
                            Debug.Assert(reg != RegisterType.Unknown);

                            string destination = GetRegisterAssembly(reg);

                            if (isWord)
                            {
                                OneOf<short, Error> imm16 = ReadS16(ref cur, streamName);
                                if (imm16.IsT1)
                                    return imm16.AsT1;

                                string source = GetValueAssembly(imm16.AsT0, outputMode);
                                assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                            }
                            else
                            {
                                OneOf<byte, Error> imm8 = ReadU8(ref cur, streamName);
                                if (imm8.IsT1)
                                    return imm8.AsT1;

                                string source = GetValueAssembly(imm8.AsT0, outputMode);
                                assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                            }
                        }
                        break;

                    case OpFamily.Arithmetic8_RegOrMem_Imm:
                    case OpFamily.Arithmetic16_RegOrMem_Imm:
                        {
                            // 8-bit/16-bit Arithmetic (ADD, SUB, OR, AND, etc.) Immediate to Register/Memory

                            bool specialCase = destinationIsRegister;

                            ArithmeticType atype = (ArithmeticType)modRegRM.RegField;

                            string destination, source;

                            if (isWord && specialCase || !isWord)
                            {
                                // @NOTE(final): 8-bit immediate, but 16-bit register or memory
                                OneOf<byte, Error> imm8 = ReadU8(ref cur, streamName);
                                if (imm8.IsT1)
                                    return imm8.AsT1;

                                if (modRegRM.Mode == Mode.RegisterMode)
                                {
                                    if (isWord)
                                        destination = GetRegisterAssembly(_regTable.GetWord(modRegRM.RMField));
                                    else
                                        destination = GetRegisterAssembly(_regTable.GetByte(modRegRM.RMField));
                                    source = GetValueAssembly(imm8.AsT0, outputMode);
                                }
                                else
                                {
                                    destination = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                                    source = GetValueAssembly(imm8.AsT0, outputMode);
                                }
                            }
                            else
                            {
                                Debug.Assert(isWord && !specialCase);

                                // @NOTE(final): 16-bit immediate and 16-bit register or memory
                                OneOf<short, Error> imm16 = ReadS16(ref cur, streamName);
                                if (imm16.IsT1)
                                    return imm16.AsT1;

                                if (modRegRM.Mode == Mode.RegisterMode)
                                {
                                    if (isWord)
                                        destination = GetRegisterAssembly(_regTable.GetWord(modRegRM.RMField));
                                    else
                                        destination = GetRegisterAssembly(_regTable.GetByte(modRegRM.RMField));
                                    source = GetValueAssembly(imm16.AsT0, outputMode);
                                }
                                else
                                {
                                    destination = GetAddressAssembly(modRegRM.EAC, displacement, outputMode);
                                    source = GetValueAssembly(imm16.AsT0, outputMode);
                                }
                            }

                            Mnemonic mnemonic = atype switch
                            {
                                ArithmeticType.Add => Mnemonics.ArithmeticAdd,
                                ArithmeticType.AddWithCarry => Mnemonics.ArithmeticAddWithCarry,
                                ArithmeticType.SubWithBorrow => Mnemonics.ArithmeticSubWithBorrow,
                                ArithmeticType.Sub => Mnemonics.ArithmeticSub,
                                ArithmeticType.Compare => Mnemonics.ArithmeticCompare,
                                _ => throw new NotSupportedException($"Arithmetic type '{atype}' is not supported for instruction '{instruction}'!")
                            };

                            assemblyLine = new AssemblyLine(mnemonic, destination, source);
                        }
                        break;

                    case OpFamily.Jump8:
                        {
                            // @NOTE(final): Jump to 8-bit offset
                            OneOf<byte, Error> inc8 = ReadU8(ref cur, streamName);
                            if (inc8.IsT1)
                                return inc8.AsT1;
                            string destination = GetAddressAssembly(inc8.AsT0, outputMode);
                            assemblyLine = assemblyLine.WithDestinationOnly(destination);
                        }
                        break;

                    case OpFamily.Unknown:
                    default:
                        return new Error(ErrorCode.InstructionNotImplemented, $"Not implemented instruction '{instruction}'!");
                }

                s.AppendLine(assemblyLine.ToString());

                Debug.WriteLine(assemblyLine.ToString());

                int delta = start.Length - cur.Length;
                if (delta < instruction.MinLength)
                    return new Error(ErrorCode.TooSmallAdvancementInStream, $"Stream '{streamName}' was not properly advanced, expect minimum advancement of '{instruction.MinLength}' but got '{delta}'!");
                if (delta > instruction.MaxLength)
                    return new Error(ErrorCode.TooLargeAdvancementInStream, $"Stream '{streamName}' was not properly advanced, expect maximum advancement of '{instruction.MaxLength}' but got '{delta}'!");
            }
            return s.ToString();
        }
    }
}
