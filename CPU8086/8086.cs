using OneOf;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Final.CPU8086
{
    public class CPU
    {
        private static readonly InstructionTable _opTable = new InstructionTable();
        private static readonly RegisterTable _regTable = new RegisterTable();
        private static readonly EffectiveAddressCalculationTable _effectiveAddressCalculationTable = new EffectiveAddressCalculationTable();

        public CPU()
        {
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

        public OneOf<Instruction, Error> DecodeNext(ReadOnlySpan<byte> stream, string streamName)
        {
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
            DataWidth dataType = isWord ? DataWidth.Word : DataWidth.Byte;
            InstructionType type = GetInstructionType(instruction.Family);
            
            switch (instruction.Family)
            {
                case OpFamily.Push_FixedReg:
                    {
                        RegisterType register = instruction.Register;
                        Debug.Assert(register != RegisterType.Unknown);
                        return new Instruction(opCode, length, type, DataWidth.Word, new InstructionOperand(register));
                    }

                case OpFamily.Pop_FixedReg:
                    {
                        RegisterType register = instruction.Register;
                        Debug.Assert(register != RegisterType.Unknown);
                        return new Instruction(opCode, length, type, DataWidth.Word, new InstructionOperand(register));
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
                        return new Instruction(opCode, length, type, DataWidth.Byte, destination, source);
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
                        return new Instruction(opCode, length, type, DataWidth.Byte, destination, source);
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
                        return new Instruction(opCode, length, type, DataWidth.Word, destination, source);
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
                        return new Instruction(opCode, length, type, DataWidth.Word, destination, source);
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

                AssemblyLine assemblyLine = new AssemblyLine(instruction.Mnemonic.Lower);

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

                            assemblyLine = new AssemblyLine(mnemonic.Lower, destination, source);
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
