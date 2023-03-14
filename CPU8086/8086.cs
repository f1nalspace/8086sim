using OneOf;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Final.CPU8086
{
    public enum InstructionDataType : byte
    {
        None = 0,
        Register,
        Memory,
        Immediate,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
    public readonly struct InstructionLocation
    {
        [FieldOffset(0)]
        public readonly InstructionDataType Type;
        [FieldOffset(1)]
        public readonly byte Length;
        [FieldOffset(2)]
        public readonly short Mem;
        [FieldOffset(2)]
        public readonly short Imm;
        [FieldOffset(2)]
        public readonly RegisterType Reg;

        private InstructionLocation(InstructionDataType type, byte u8)
        {
            Type = type;
            Length = 1;
            Reg = RegisterType.Unknown;
            Imm = u8;
            Mem = u8;
        }

        private InstructionLocation(InstructionDataType type, short s16)
        {
            Type = type;
            Length = 2;
            Reg = RegisterType.Unknown;
            Imm = s16;
            Mem = s16;
        }

        private InstructionLocation(InstructionDataType type, RegisterType reg)
        {
            Type = type;
            Length = Register.GetLength(reg);
            Imm = 0;
            Mem = 0;
            Reg = reg;
        }

        public override string ToString()
        {
            if (Type == InstructionDataType.Register)
                return Register.GetName(Reg);
            else if (Type == InstructionDataType.Memory)
                return Mem.ToString("[X]");
            else if (Type == InstructionDataType.Immediate)
                return Imm.ToString("X");
            else
                return "None";
        }

        public static InstructionLocation AsMemory(short memory)
            => new InstructionLocation(InstructionDataType.Memory, memory);

        public static InstructionLocation AsImmediate(short imm)
            => new InstructionLocation(InstructionDataType.Immediate, imm);

        public static InstructionLocation AsRegister(RegisterType reg)
            => new InstructionLocation(InstructionDataType.Register, reg);
    }

    public readonly struct InstructionData
    {
        public OpCode OpCode { get; }
        public byte Length { get; }
        public InstructionLocation Dest { get; }
        public InstructionLocation Source { get; }

        public InstructionData(OpCode opCode, byte length, InstructionLocation dest, InstructionLocation source)
        {
            OpCode = opCode;
            Length = length;
            Dest = dest;
            Source = source;
        }

        public override string ToString()
        {
            if (Dest.Type != InstructionDataType.None && Source.Type != InstructionDataType.None)
                return $"{OpCode} {Dest}, {Source} ({Length} bytes)";
            else if (Dest.Type != InstructionDataType.None && Source.Type == InstructionDataType.None)
                return $"{OpCode} {Dest} ({Length} bytes)";
            else
                return $"{OpCode} ({Length} bytes)";
        }
    }

    public delegate InstructionData DecodeInstructionEventHandler(ref ReadOnlySpan<byte> stream);

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

        private static OneOf<short, Error> LoadDisplacementOrZero(EffectiveAddressCalculation eac, ref ReadOnlySpan<byte> stream, string streamName)
        {
            byte len = eac switch
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

            if (len == 1)
            {
                OneOf<byte, Error> u8 = ReadU8(ref stream, streamName);
                if (u8.IsT1)
                    return new Error(u8.AsT1, $"Cannot load 8-bit displacement for EAC '{eac}'");
                return u8.AsT0;
            }
            else if (len == 2)
            {
                OneOf<short, Error> s16 = ReadS16(ref stream, streamName);
                if (s16.IsT1)
                    return new Error(s16.AsT1, $"Cannot load 16-bit displacement for EAC '{eac}'");
                return s16.AsT0;
            }
            else
                return 0;
        }

        private static (string destination, string source) GetDestinationAndSource(ModRegRM modRegRM, bool directionIsToRegister, bool isWord, short displacement, OutputValueMode outputMode)
        {
            string destination, source;
            if (modRegRM.Mode == Mode.RegisterMode)
            {
                if (isWord)
                {
                    // 16-bit Register to Register
                    if (directionIsToRegister)
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
                    if (directionIsToRegister)
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
                    if (directionIsToRegister)
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
                    if (directionIsToRegister)
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

                Instruction instruction = _opTable[opCode];
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
                    OneOf<short, Error> displacementRes = LoadDisplacementOrZero(modRegRM.EAC, ref cur, streamName);
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
                            // Push/Pop Fixed Register
                            string destination = GetRegisterAssembly(RegisterType.ES);
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
                                ArithmeticType.Add => Mnemonics.Add,
                                ArithmeticType.AddWithCarry => Mnemonics.AddWithCarry,
                                ArithmeticType.SubWithBorrow => Mnemonics.SubWithBorrow,
                                ArithmeticType.Sub => Mnemonics.Sub,
                                ArithmeticType.Compare => Mnemonics.Cmp,
                                _ => throw new NotSupportedException($"Arithmetic type '{atype}' is not supported for instruction '{instruction}'!")
                            };

                            assemblyLine = new AssemblyLine(mnemonic.Lower, destination, source);
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
