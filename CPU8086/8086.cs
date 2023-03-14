using OneOf;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CPU8086
{
    public readonly struct Mnemonic
    {
        public string Upper { get; }
        public string Lower { get; }

        public Mnemonic(string upper, string lower)
        {
            Upper = upper;
            Lower = lower;
        }

        public override string ToString() => Upper;
    }

    static class Mnemonics
    {
        public static readonly Mnemonic Push = new Mnemonic("PUSH", "push");
        public static readonly Mnemonic Pop = new Mnemonic("POP", "pop");

        public static readonly Mnemonic Mov = new Mnemonic("MOV", "mov");

        public static readonly Mnemonic Add = new Mnemonic("ADD", "add");
        public static readonly Mnemonic Or = new Mnemonic("OR", "or");
        public static readonly Mnemonic AddWithCarry = new Mnemonic("ADC", "adc");
        public static readonly Mnemonic SubWithBorrow = new Mnemonic("SBB", "sbb");
        public static readonly Mnemonic And = new Mnemonic("AND", "and");
        public static readonly Mnemonic Sub = new Mnemonic("SUB", "sub");
        public static readonly Mnemonic Xor = new Mnemonic("XOR", "xor");
        public static readonly Mnemonic Cmp = new Mnemonic("CMP", "cmp");

        public static readonly Mnemonic Dynamic = new Mnemonic("(DYNAMIC)", "(dynamic)");
    }

    public enum ErrorCode
    {
        Unknown = 0,
        NotEnoughBytesInStream,
        TooSmallAdvancementInStream,
        TooLargeAdvancementInStream,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionMissingAdditionalData,
        ModeNotImplemented,
    }

    public readonly struct Error
    {
        public ErrorCode Code { get; }
        public string Message { get; }

        public Error(ErrorCode code, string message)
        {
            Code = code;
            Message = message;
            Debug.Fail(message);
        }

        public Error(Error error, string message)
        {
            Code = error.Code;
            Message = $"{message}: {error.Message}";
            Debug.Fail(message);
        }

        public override string ToString() => $"[{Code}] {Message}";
    }

    public enum OutputValueMode
    {
        AsInteger = 0,
        AsHex8,
        AsHex16,
        AsHexAuto,
    }

    public enum RegisterType : byte
    {
        /// <summary>
        /// Unknown register
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 16-bit accumulator register (AX)
        /// </summary>
        AX,
        /// <summary>
        /// 8-bit low accumulator register (AH)
        /// </summary>
        AL,
        /// <summary>
        /// 8-bit high accumulator register (AH)
        /// </summary>
        AH,

        /// <summary>
        /// 16-bit base register (BX)
        /// </summary>
        BX,
        /// <summary>
        /// 8-bit low base register (BL)
        /// </summary>
        BL,
        /// <summary>
        /// 8-bit high base register (BL)
        /// </summary>
        BH,

        /// <summary>
        /// 16-bit counting register (CX)
        /// </summary>
        CX,
        /// <summary>
        /// 8-bit low counting register (CL)
        /// </summary>
        CL,
        /// <summary>
        /// 8-bit high counting register (CL)
        /// </summary>
        CH,

        /// <summary>
        /// 16-bit data register (DX)
        /// </summary>
        DX,
        /// <summary>
        /// 8-bit low data register (DL)
        /// </summary>
        DL,
        /// <summary>
        /// 8-bit high data register (DL)
        /// </summary>
        DH,

        /// <summary>
        /// 16-bit stack pointer (SP)
        /// </summary>
        SP,
        /// <summary>
        /// 16-bit base pointer (BP)
        /// </summary>
        BP,
        /// <summary>
        /// 16-bit source index (SI)
        /// </summary>
        SI,
        /// <summary>
        /// 16-bit destination index (DI)
        /// </summary>
        DI,

        /// <summary>
        /// 16-bit code segment (CS)
        /// </summary>
        CS,
        /// <summary>
        /// 16-bit data segment (DS)
        /// </summary>
        DS,
        /// <summary>
        /// 16-bit stack segment (SS)
        /// </summary>
        SS,
        /// <summary>
        /// 16-bit extra segment (ES)
        /// </summary>
        ES,
    }

    public enum Mode : byte
    {
        MemoryNoDisplacement = 0b00,
        MemoryByteDisplacement = 0b01,
        MemoryWordDisplacement = 0b10,
        RegisterMode = 0b11,
    }

    public enum ArithmeticType : byte
    {
        Add = 0b000,
        Or = 0b001,
        AddWithCarry = 0b010,
        SubWithBorrow = 0b011,
        And = 0b100,
        Sub = 0b101,
        Xor = 0b110,
        Compare = 0b111,
    }

    public class Register
    {
        public byte Code { get; }
        public RegisterType Type { get; }

        public Register(byte code, RegisterType type)
        {
            Code = code;
            Type = type;
        }

        public static byte GetLength(RegisterType name)
        {
            return name switch
            {
                RegisterType.AX => 2,
                RegisterType.AL => 1,
                RegisterType.AH => 1,

                RegisterType.BX => 2,
                RegisterType.BL => 1,
                RegisterType.BH => 1,

                RegisterType.CX => 2,
                RegisterType.CL => 1,
                RegisterType.CH => 1,

                RegisterType.DX => 2,
                RegisterType.DL => 1,
                RegisterType.DH => 1,

                RegisterType.SP => 2,
                RegisterType.BP => 2,
                RegisterType.SI => 2,
                RegisterType.DI => 2,

                RegisterType.CS => 2,
                RegisterType.DS => 2,
                RegisterType.SS => 2,
                RegisterType.ES => 2,

                _ => 0
            };
        }

        public static string GetLowerName(RegisterType name)
        {
            return name switch
            {
                RegisterType.AX => "ax",
                RegisterType.AL => "al",
                RegisterType.AH => "ah",

                RegisterType.BX => "bx",
                RegisterType.BL => "bl",
                RegisterType.BH => "bh",

                RegisterType.CX => "cx",
                RegisterType.CL => "cl",
                RegisterType.CH => "ch",

                RegisterType.DX => "dx",
                RegisterType.DL => "dl",
                RegisterType.DH => "dh",

                RegisterType.SP => "sp",
                RegisterType.BP => "bp",
                RegisterType.SI => "si",
                RegisterType.DI => "di",

                RegisterType.CS => "cs",
                RegisterType.DS => "ds",
                RegisterType.SS => "ss",
                RegisterType.ES => "es",

                _ => null
            };
        }

        public static string GetName(RegisterType name)
        {
            return name switch
            {
                RegisterType.AX => nameof(RegisterType.AX),
                RegisterType.AL => nameof(RegisterType.AL),
                RegisterType.AH => nameof(RegisterType.AH),

                RegisterType.BX => nameof(RegisterType.BX),
                RegisterType.BL => nameof(RegisterType.BL),
                RegisterType.BH => nameof(RegisterType.BH),

                RegisterType.CX => nameof(RegisterType.CX),
                RegisterType.CL => nameof(RegisterType.CL),
                RegisterType.CH => nameof(RegisterType.CH),

                RegisterType.DX => nameof(RegisterType.DX),
                RegisterType.DL => nameof(RegisterType.DL),
                RegisterType.DH => nameof(RegisterType.DH),

                RegisterType.SP => nameof(RegisterType.SP),
                RegisterType.BP => nameof(RegisterType.BP),
                RegisterType.SI => nameof(RegisterType.SI),
                RegisterType.DI => nameof(RegisterType.DI),

                RegisterType.CS => nameof(RegisterType.CS),
                RegisterType.DS => nameof(RegisterType.DS),
                RegisterType.SS => nameof(RegisterType.SS),
                RegisterType.ES => nameof(RegisterType.ES),

                _ => null
            };
        }

        public override string ToString() => GetName(Type);
    }

    public enum OpCode : int
    {
        Unknown = -1,

        ADD_dREG8_dMEM8_sREG8 = 0x00,
        ADD_dREG16_dMEM16_sREG16 = 0x01,
        ADD_dREG8_sREG8_sMEM8 = 0x02,
        ADD_dREG16_sREG16_sMEM16 = 0x03,
        ADD_dAL_sIMM8 = 0x04,
        ADD_dAX_sIMM16 = 0x05,

        PUSH_ES = 0x06,
        POP_ES = 0x07,

        OR_dREG8_dMEM8_sREG8 = 0x08,
        OR_dREG16_dMEM16_sREG16 = 0x09,
        OR_dREG8_sREG8_sMEM8 = 0x0A,
        OR_dREG16_sREG16_sMEM16 = 0x0B,
        OR_dAL_sIMM8 = 0x0C,
        OR_dAX_sIMM16 = 0x0D,

        PUSH_CS = 0x0E,
        // Unused = 0x0F,

        /// <summary>
        /// <para>8-bit Arithmetic instructions such as ADD, ADC, SBB, SUB, CMP, etc. writing to register or memory, using an immediate as source.</para>
        /// <para>REG field decides what type of arithmetic operating it actually is.</para>
        /// </summary>
        ARITHMETIC_dREG8_dMEM8_sIMM8 = 0x80,

        /// <summary>
        /// <para>16-bit Arithmetic instructions such as ADD, ADC, SBB, SUB, CMP, etc. writing to register or memory, using an immediate as source.</para>
        /// <para>REG field decides what type of arithmetic operating it actually is.</para>
        /// </summary>
        ARITHMETIC_dREG16_dMEM16_sIMM16 = 0x83,

        MOV_dREG8_dMEM8_sREG8 = 0x88,
        MOV_dREG16_dMEM16_sREG16 = 0x89,
        MOV_dREG8_sMEM8_sREG8 = 0x8A,
        MOV_dREG16_sMEM16_sREG16 = 0x8B,

        MOV_dAL_sMEM8 = 0xA0,
        MOV_dAX_sMEM16 = 0xA1,
        MOV_dMEM8_sAL = 0xA2,
        MOV_dMEM16_sAX = 0xA3,

        MOV_dAL_sIMM8 = 0xB0,
        MOV_dCL_sIMM8 = 0xB1,
        MOV_dDL_sIMM8 = 0xB2,
        MOV_dBL_sIMM8 = 0xB3,
        MOV_dAH_sIMM8 = 0xB4,
        MOV_dCH_sIMM8 = 0xB5,
        MOV_dDH_sIMM8 = 0xB6,
        MOV_BH_IMM8 = 0xB7,
        MOV_dAX_sIMM16 = 0xB8,
        MOV_dCX_sIMM16 = 0xB9,
        MOV_dDX_sIMM16 = 0xBA,
        MOV_dBX_sIMM16 = 0xBB,
        MOV_dSP_sIMM16 = 0xBC,
        MOV_dBP_sIMM16 = 0xBD,
        MOV_dSI_sIMM16 = 0xBE,
        MOV_dDI_sIMM16 = 0xBF,

        MOV_dMEM8_sIMM8 = 0xC6,
        MOV_dMEM16_sIMM16 = 0xC7,
    }

    public enum OpFamily : byte
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Decrements the <see cref="RegisterType.SP"/> by the value of a fixed register
        /// </summary>
        Push_FixedReg,
        /// <summary>
        /// Increments the <see cref="RegisterType.SP"/> by the value of a fixed register
        /// </summary>
        Pop_FixedReg,

        /// <summary>
        /// Move 8-bit register/memory to 8-bit register/memory (MOV REG8/MEM8, REG8/MEM8)
        /// </summary>
        Move8_RegOrMem_RegOrMem,
        /// <summary>
        /// Move 16-bit register/memory to 16-bit register/memory (MOV REG16/MEM16, REG16/MEM16)
        /// </summary>
        Move16_RegOrMem_RegOrMem,

        /// <summary>
        /// Move 8-bit immediate to 8-bit register (MOV REG8, IMM8)
        /// </summary>
        Move8_Reg_Imm,
        /// <summary>
        /// Move 16-bit immediate to 16-bit register (MOV REG16, IMM16)
        /// </summary>
        Move16_Reg_Imm,

        /// <summary>
        /// Move 8-bit immediate to 8-bit memory (MOV MEM8, IMM8)
        /// </summary>
        Move8_Mem_Imm,
        /// <summary>
        /// Move 16-bit immediate to 16-bit memory (MOV MEM16, IMM16)
        /// </summary>
        Move16_Mem_Imm,

        /// <summary>
        /// Move 8-bit memory to 8-bit fixed register (MOV AL, MEM8)
        /// </summary>
        Move8_FixedReg_Mem,
        /// <summary>
        /// Move 16-bit memory to 16-bit fixed register (MOV AX, MEM16)
        /// </summary>
        Move16_FixedReg_Mem,

        /// <summary>
        /// Move 8-bit fixed register to 168bit memory (MOV MEM8, AL)
        /// </summary>
        Move8_Mem_FixedReg,
        /// <summary>
        /// Move 16-bit fixed register to 16-bit memory (MOV MEM16, AX)
        /// </summary>
        Move16_Mem_FixedReg,

        /// <summary>
        /// Add 8-bit register to 8-bit register/memory (ADD REG8/MEM8, REG8)
        /// </summary>
        Add8_RegOrMem_RegOrMem,
        /// <summary>
        /// Add 16-bit register to 16-bit register/memory (ADD REG16/MEM16, REG16)
        /// </summary>
        Add16_RegOrMem_RegOrMem,
        /// <summary>
        /// Add 8-bit immediate to 8-bit fixed register (ADD AL, IMM8)
        /// </summary>
        Add8_FixedReg_Imm,
        /// <summary>
        /// Add 16-bit immediate to 16-bit fixed register (ADD AX, IMM8)
        /// </summary>
        Add16_FixedReg_Imm,

        /// <summary>
        /// Logical OR to 8-bit register/memory with 8-bit immediate (OR AL, BH)
        /// </summary>
        Or8_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical OR to 16-bit register/memory with 16-bit immediate (OR AX, BX)
        /// </summary>
        Or16_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical OR to 8-bit fixed register with 8-bit immediate (OR AL, IMM8)
        /// </summary>
        Or8_FixedReg_Imm,
        /// <summary>
        /// Logical OR to 16-bit fixed register with 16-bit immediate (OR AX, IMM16)
        /// </summary>
        Or16_FixedReg_Imm,

        /// <summary>
        /// <para>Arithmetic operation 8-bit immediate to 8-bit register/memory (ADD REG8/MEM8, IMM8).</para>
        /// <para>REG field decides which mathmatical instruction it is:</para>
        /// <para>REG (000) = ADD</para>
        /// <para>REG (001) = OR</para>
        /// <para>REG (010) = ADC</para>
        /// <para>REG (011) = SBB</para>
        /// <para>REG (100) = AND</para>
        /// <para>REG (101) = SUB</para>
        /// <para>REG (110) = XOR</para>
        /// <para>REG (111) = CMP</para>
        /// </summary>
        Arithmetic8_RegOrMem_Imm,

        /// <summary>
        /// <para>Arithmetic operation 16-bit immediate to 16-bit register/memory (ADD REG16/MEM16, IMM16).</para>
        /// <para>REG field decides which mathmatical instruction it is:</para>
        /// <para>REG (000) = ADD</para>
        /// <para>REG (001) = Not used</para>
        /// <para>REG (010) = ADC</para>
        /// <para>REG (011) = SBB</para>
        /// <para>REG (100) = Not used</para>
        /// <para>REG (101) = SUB</para>
        /// <para>REG (110) = Not used</para>
        /// <para>REG (111) = CMP</para>
        /// </summary>
        Arithmetic16_RegOrMem_Imm,
    }

    public enum EffectiveAddressCalculation : byte
    {
        None = 0,

        // Mod == 00
        BX_SI,
        BX_DI,
        BP_SI,
        BP_DI,
        SI,
        DI,
        DirectAddress,
        BX,

        // Mod == 01
        BX_SI_D8,
        BX_DI_D8,
        BP_SI_D8,
        BP_DI_D8,
        SI_D8,
        DI_D8,
        BP_D8,
        BX_D8,

        // Mod == 10
        BX_SI_D16,
        BX_DI_D16,
        BP_SI_D16,
        BP_DI_D16,
        SI_D16,
        DI_D16,
        BP_D16,
        BX_D16,
    }

    [Flags]
    public enum FieldEncoding
    {
        None = 0,
        ModRemRM = 1 << 0,
    }

    public class RegisterTable
    {
        private readonly Register[] _table;

        public RegisterTable()
        {
            _table = new Register[16];

            _table[0] = new Register(0b000, RegisterType.AL);
            _table[1] = new Register(0b001, RegisterType.CL);
            _table[2] = new Register(0b010, RegisterType.DL);
            _table[3] = new Register(0b011, RegisterType.BL);
            _table[4] = new Register(0b100, RegisterType.AH);
            _table[5] = new Register(0b101, RegisterType.CH);
            _table[6] = new Register(0b110, RegisterType.DH);
            _table[7] = new Register(0b111, RegisterType.BH);

            _table[8] = new Register(0b000, RegisterType.AX);
            _table[9] = new Register(0b001, RegisterType.CX);
            _table[10] = new Register(0b010, RegisterType.DX);
            _table[11] = new Register(0b011, RegisterType.BX);
            _table[12] = new Register(0b100, RegisterType.SP);
            _table[13] = new Register(0b101, RegisterType.BP);
            _table[14] = new Register(0b110, RegisterType.SI);
            _table[15] = new Register(0b111, RegisterType.DI);
        }

        public ref readonly Register GetByte(byte index) => ref _table[index];
        public ref readonly Register GetWord(byte index) => ref _table[index + 8];
    }

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

    public class Instruction
    {
        public OpCode OpCode { get; }
        public OpFamily Family { get; }
        public FieldEncoding Encoding { get; }
        public RegisterType Register { get; }
        public byte MinLength { get; }
        public byte MaxLength { get; }
        public Mnemonic Mnemonic { get; }
        public string Description { get; }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, RegisterType register, byte minLength, byte maxLength, Mnemonic mnemonic, string description)
        {
            if (opCode == OpCode.Unknown)
                throw new ArgumentNullException(nameof(opCode));
            if (family == OpFamily.Unknown)
                throw new ArgumentNullException(nameof(family));
            if (minLength < 1 || minLength > 6)
                throw new ArgumentOutOfRangeException(nameof(minLength), minLength, $"Instruction min-length '{maxLength}' can only be in range of 1 - 6!");
            if (maxLength < 1 || maxLength > 6)
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, $"Instruction max-length '{maxLength}' can only be in range of 1 - 6!");
            if (maxLength < minLength)
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, $"Instruction max-length '{maxLength}' must be greater than min-length of '{minLength}'!");
            OpCode = opCode;
            Family = family;
            Encoding = encoding;
            Register = register;
            MinLength = minLength;
            MaxLength = maxLength;
            Mnemonic = mnemonic;
            Description = description;
        }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, byte minLength, byte maxLength, Mnemonic mnemonic, string description) : this(opCode, family, encoding, RegisterType.Unknown, minLength, maxLength, mnemonic, description)
        {
        }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, RegisterType register, byte minLength, Mnemonic mnemonic, string description) : this(opCode, family, encoding, register, minLength, minLength, mnemonic, description)
        {
        }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, byte minLength, Mnemonic mnemonic, string description) : this(opCode, family, encoding, RegisterType.Unknown, minLength, minLength, mnemonic, description)
        {
        }

        public override string ToString()
        {
            if (MinLength == MaxLength)
                return $"{OpCode}/{((byte)OpCode).ToBinary()} ({MinLength} bytes, family: {Family}, encoding: {Encoding}, register: {Register})";
            else
                return $"{OpCode}/{((byte)OpCode).ToBinary()} ({MinLength} to {MaxLength} bytes, family: {Family}, encoding: {Encoding}, register: {Register})";
        }
    }

    public class EffectiveAddressCalculationTable
    {
        // R/M + MOD = 8 * 3
        public readonly EffectiveAddressCalculation[] _table;
        private readonly byte[] _displacementLengths;

        public EffectiveAddressCalculationTable()
        {
            _table = new EffectiveAddressCalculation[8 * 3];

            _table[0b00 * 8 + 0b000] = EffectiveAddressCalculation.BX_SI;
            _table[0b00 * 8 + 0b001] = EffectiveAddressCalculation.BX_DI;
            _table[0b00 * 8 + 0b010] = EffectiveAddressCalculation.BP_SI;
            _table[0b00 * 8 + 0b011] = EffectiveAddressCalculation.BP_DI;
            _table[0b00 * 8 + 0b100] = EffectiveAddressCalculation.SI;
            _table[0b00 * 8 + 0b101] = EffectiveAddressCalculation.DI;
            _table[0b00 * 8 + 0b110] = EffectiveAddressCalculation.DirectAddress;
            _table[0b00 * 8 + 0b111] = EffectiveAddressCalculation.BX;

            _table[0b01 * 8 + 0b000] = EffectiveAddressCalculation.BX_SI_D8;
            _table[0b01 * 8 + 0b001] = EffectiveAddressCalculation.BX_DI_D8;
            _table[0b01 * 8 + 0b010] = EffectiveAddressCalculation.BP_SI_D8;
            _table[0b01 * 8 + 0b011] = EffectiveAddressCalculation.BP_DI_D8;
            _table[0b01 * 8 + 0b100] = EffectiveAddressCalculation.SI_D8;
            _table[0b01 * 8 + 0b101] = EffectiveAddressCalculation.DI_D8;
            _table[0b01 * 8 + 0b110] = EffectiveAddressCalculation.BP_D8;
            _table[0b01 * 8 + 0b111] = EffectiveAddressCalculation.BX_D8;

            _table[0b10 * 8 + 0b000] = EffectiveAddressCalculation.BX_SI_D16;
            _table[0b10 * 8 + 0b001] = EffectiveAddressCalculation.BX_DI_D16;
            _table[0b10 * 8 + 0b010] = EffectiveAddressCalculation.BP_SI_D16;
            _table[0b10 * 8 + 0b011] = EffectiveAddressCalculation.BP_DI_D16;
            _table[0b10 * 8 + 0b100] = EffectiveAddressCalculation.SI_D16;
            _table[0b10 * 8 + 0b101] = EffectiveAddressCalculation.DI_D16;
            _table[0b10 * 8 + 0b110] = EffectiveAddressCalculation.BP_D16;
            _table[0b10 * 8 + 0b111] = EffectiveAddressCalculation.BX_D16;

            _displacementLengths = new byte[32];

            _displacementLengths[(byte)EffectiveAddressCalculation.BX_SI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_DI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_SI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_DI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.SI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.DI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_D8] = 1;

            _displacementLengths[(byte)EffectiveAddressCalculation.BX_SI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_DI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_SI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_DI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.SI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.DI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.DirectAddress] = 2;
        }

        public EffectiveAddressCalculation Get(byte rm, byte mod)
        {
            byte index = (byte)(mod * 8 + rm);
            return _table[index];
        }

        public byte GetDisplacementLength(EffectiveAddressCalculation eac) => _displacementLengths[(byte)eac];
    }

    public class InstructionTable
    {
        private readonly Instruction[] _table;

        public ref readonly Instruction this[int index] => ref _table[index];

        public InstructionTable()
        {
            _table = new Instruction[256];

            // Instruction Encoding (8086 Family Users Manual v1)
            // We dont want to test for invidiual bits, we rather want to map the full opcode directly

            //
            // Register Field Encoding (Table 4-9, Page: 162)
            //
            // |  REG  | W = 0 | W = 1 |
            // | 0 0 0 |  AL   | AX    |
            // | 0 0 1 |  CL   | CX    |
            // | 0 1 0 |  DL   | DX    |
            // | 0 1 1 |  BL   | BX    |
            // | 1 0 0 |  AH   | SP    |
            // | 1 0 1 |  CH   | BP    |
            // | 1 1 0 |  DH   | SI    |
            // | 1 1 1 |  BH   | DI    |

            //
            // Move instructions (Table 4-12, Page: 164 or 174-175)
            //
            // Bug(Page 174) $A3: MOV16, AL is wrong, use MOV16, AX instead
            //

            // 00000000 to 00000111 (ADD)
            _table[0x00 /* 0000 0000 */] = new Instruction(OpCode.ADD_dREG8_dMEM8_sREG8, OpFamily.Add8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 8-bit Register to 8-bit Register/Memory");
            _table[0x01 /* 0000 0001 */] = new Instruction(OpCode.ADD_dREG16_dMEM16_sREG16, OpFamily.Add16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 16-bit Register to 16-bit Register/Memory");
            _table[0x02 /* 0000 0010 */] = new Instruction(OpCode.ADD_dREG8_sREG8_sMEM8, OpFamily.Add8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 8-bit Register/Memory to 8-bit Register");
            _table[0x03 /* 0000 0011 */] = new Instruction(OpCode.ADD_dREG16_sREG16_sMEM16, OpFamily.Add16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 16-bit Register/Memory to 16-bit Register");
            _table[0x04 /* 0000 0100 */] = new Instruction(OpCode.ADD_dAL_sIMM8, OpFamily.Add8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.Add, "Adds 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0x05 /* 0000 0101 */] = new Instruction(OpCode.ADD_dAX_sIMM16, OpFamily.Add16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Add, "Adds 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");

            // 00000110 to 00000111 (PUSH/POP)
            _table[0x06 /* 0000 0110 */] = new Instruction(OpCode.PUSH_ES, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.ES, 1, Mnemonics.Push, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.ES + " Register");
            _table[0x07 /* 0000 0111 */] = new Instruction(OpCode.POP_ES, OpFamily.Pop_FixedReg, FieldEncoding.None, RegisterType.ES, 1, Mnemonics.Pop, "Increments the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.ES + " Register");

            // 00001000 to 0000 1111
            _table[0x08 /* 0000 1000 */] = new Instruction(OpCode.OR_dREG8_dMEM8_sREG8, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 8-bit Register with 8-bit Register/Memory");
            _table[0x09 /* 0000 1001 */] = new Instruction(OpCode.OR_dREG16_dMEM16_sREG16, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 16-bit Register with 16-bit Register/Memory");
            _table[0x0A /* 0000 1010 */] = new Instruction(OpCode.OR_dREG8_sREG8_sMEM8, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 8-bit Register/Memory with 8-bit Register");
            _table[0x0B /* 0000 1011 */] = new Instruction(OpCode.OR_dREG16_sREG16_sMEM16, OpFamily.Or16_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 16-bit Register/Memory with 16-bit Register");
            _table[0x0C /* 0000 1100 */] = new Instruction(OpCode.OR_dAL_sIMM8, OpFamily.Or8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.Or, "Logical or 8-bit Immediate with 8-bit " + RegisterType.AL + " Register");
            _table[0x0D /* 0000 1101 */] = new Instruction(OpCode.OR_dAX_sIMM16, OpFamily.Or16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Or, "Logical or 16-bit Immediate with 16-bit " + RegisterType.AX + " Register");
            _table[0x0E /* 0000 1110 */] = new Instruction(OpCode.PUSH_CS, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.CS, 1, Mnemonics.Or, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.CS + " Register");
            _table[0x0F /* 0000 1111 */] = null; // Unused

            // 10000000 (ADD/ADC/SUB,etc.)
            _table[0x80 /* 1000 0000 */] = new Instruction(OpCode.ARITHMETIC_dREG8_dMEM8_sIMM8, OpFamily.Arithmetic8_RegOrMem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Dynamic, "Arithmetic 8-bit Immediate to 8-bit Register/Memory");

            // 10000011 (ADD/ADC/SUB,etc.)
            _table[0x83 /* 1000 0011 */] = new Instruction(OpCode.ARITHMETIC_dREG16_dMEM16_sIMM16, OpFamily.Arithmetic16_RegOrMem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Dynamic, "Arithmetic 16-bit Immediate to 16-bit Register/Memory");

            // 1 0 0 0 1 0 d w (MOV)
            _table[0x88 /* 100010 00 */] = new Instruction(OpCode.MOV_dREG8_dMEM8_sREG8, OpFamily.Move8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 8-bit Register to 8-bit Register/Memory");
            _table[0x89 /* 100010 01 */] = new Instruction(OpCode.MOV_dREG16_dMEM16_sREG16, OpFamily.Move16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 16-bit Register to 16-bit Register/Memory");
            _table[0x8A /* 100010 10 */] = new Instruction(OpCode.MOV_dREG8_sMEM8_sREG8, OpFamily.Move8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 8-bit Register/Memory to 8-bit Register");
            _table[0x8B /* 100010 11 */] = new Instruction(OpCode.MOV_dREG16_sMEM16_sREG16, OpFamily.Move16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 8-bit Register/Memory to 8-bit Register");

            // 10100000 to 10100011 (MOV)
            _table[0xA0 /* 1010000 */] = new Instruction(OpCode.MOV_dAL_sMEM8, OpFamily.Move8_FixedReg_Mem, FieldEncoding.None, RegisterType.AL, 3, Mnemonics.Mov, "Copy 8-bit Memory to 8-bit " + RegisterType.AL + " Register");
            _table[0xA1 /* 1010001 */] = new Instruction(OpCode.MOV_dAX_sMEM16, OpFamily.Move16_FixedReg_Mem, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Mov, "Copy 16-bit Memory to 16-bit " + RegisterType.AX + " Register");
            _table[0xA2 /* 1010010 */] = new Instruction(OpCode.MOV_dMEM8_sAL, OpFamily.Move8_Mem_FixedReg, FieldEncoding.None, RegisterType.AL, 3, Mnemonics.Mov, "Copy 8-bit " + RegisterType.AL + " Register to 8-bit Memory");
            _table[0xA3 /* 1010011 */] = new Instruction(OpCode.MOV_dMEM16_sAX, OpFamily.Move16_Mem_FixedReg, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Mov, "Copy 16-bit " + RegisterType.AX + " Register to 16-bit Memory"); // BUG(final): Page 174, instruction $A3: MOV16, AL is wrong, correct is MOV16, AX

            // 1 0 1 1 w reg (MOV)
            _table[0xB0 /* 1011 0 000 */] = new Instruction(OpCode.MOV_dAL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0xB1 /* 1011 0 001 */] = new Instruction(OpCode.MOV_dCL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.CL + " Register");
            _table[0xB2 /* 1011 0 010 */] = new Instruction(OpCode.MOV_dDL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.DL + " Register");
            _table[0xB3 /* 1011 0 011 */] = new Instruction(OpCode.MOV_dBL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.BL + " Register");
            _table[0xB4 /* 1011 0 100 */] = new Instruction(OpCode.MOV_dAH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.AH + " Register");
            _table[0xB5 /* 1011 0 101 */] = new Instruction(OpCode.MOV_dCH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.CH + " Register");
            _table[0xB6 /* 1011 0 110 */] = new Instruction(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.DH + " Register");
            _table[0xB7 /* 1011 0 111 */] = new Instruction(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.BH + " Register");
            _table[0xB8 /* 1011 1 000 */] = new Instruction(OpCode.MOV_dAX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");
            _table[0xB9 /* 1011 1 001 */] = new Instruction(OpCode.MOV_dCX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.CX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.CX + " Register");
            _table[0xBA /* 1011 1 010 */] = new Instruction(OpCode.MOV_dDX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.DX + " Register");
            _table[0xBB /* 1011 1 011 */] = new Instruction(OpCode.MOV_dBX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.BX + " Register");
            _table[0xBC /* 1011 1 100 */] = new Instruction(OpCode.MOV_dSP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SP, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.SP + " Register");
            _table[0xBD /* 1011 1 101 */] = new Instruction(OpCode.MOV_dBP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BP, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.BP + " Register");
            _table[0xBE /* 1011 1 110 */] = new Instruction(OpCode.MOV_dSI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SI, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.SI + " Register");
            _table[0xBF /* 1011 1 111 */] = new Instruction(OpCode.MOV_dDI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DI, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.DI + " Register");

            // 11000110 to 11000111 (MOV)
            _table[0xC6 /* 1100 0110 */] = new Instruction(OpCode.MOV_dMEM8_sIMM8, OpFamily.Move8_Mem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit Memory");
            _table[0xC7 /* 1100 0111 */] = new Instruction(OpCode.MOV_dMEM16_sIMM16, OpFamily.Move16_Mem_Imm, FieldEncoding.ModRemRM, 4, 6, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit Memory");
        }
    }

    public readonly struct AssemblyLine
    {
        public string Mnemonic { get; }
        public string Destination { get; }
        public string Source { get; }

        public AssemblyLine(string mnemonic, string destination = null, string source = null)
        {
            Mnemonic = mnemonic;
            Destination = destination;
            Source = source;
        }

        public AssemblyLine WithDestinationAndSource(string destination, string source)
            => new AssemblyLine(Mnemonic, destination, source);

        public AssemblyLine WithDestinationOnly(string destination)
            => new AssemblyLine(Mnemonic, destination, null);

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Destination) && !string.IsNullOrEmpty(Source))
                return $"{Mnemonic} {Destination}, {Source}";
            else if (!string.IsNullOrEmpty(Destination))
                return $"{Mnemonic} {Destination}";
            else
                return Mnemonic;
        }
    }

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
            byte modField = (byte)((value >> 6) & 0b111);
            byte regField = (byte)((value >> 3) & 0b111);
            byte rmField = (byte)((value >> 0) & 0b111);
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

        private static (string destination, string source)GetDestinationAndSource(ModRegRM modRegRM, bool directionIsToRegister, bool isWord, short displacement, OutputValueMode outputMode)
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
                            string destination = GetRegisterAssembly(RegisterType.ES);
                            assemblyLine = assemblyLine.WithDestinationOnly(destination);
                        }
                        break;

                    case OpFamily.Move8_RegOrMem_RegOrMem:
                    case OpFamily.Move16_RegOrMem_RegOrMem:
                        {
                            (string destination, string source) = GetDestinationAndSource(modRegRM, destinationIsRegister, isWord, displacement, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Move8_Reg_Imm:
                        {
                            // 8-bit Immediate To Register
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
                            // 8-bit Explicit Immediate to Memory
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
                            // 16-bit Immediate To Register
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
                            // 16-bit Explicit Immediate to Memory
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
                            bool destinationIsMemory = !destinationIsRegister;

                            // 8-bit/16-bit Memory to Fixed-Register or 8-bit/16-bit Fixed-Register to Memory

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
                        {
                            (string destination, string source) = GetDestinationAndSource(modRegRM, destinationIsRegister, isWord, displacement, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Add8_FixedReg_Imm:
                        {
                            OneOf<byte, Error> imm8 = ReadU8(ref cur, streamName);
                            if (imm8.IsT1)
                                return imm8.AsT1;

                            RegisterType reg = instruction.Register;
                            Debug.Assert(reg != RegisterType.Unknown);

                            string destination = GetRegisterAssembly(reg);
                            string source = GetValueAssembly(imm8.AsT0, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;
                    case OpFamily.Add16_FixedReg_Imm:
                        {
                            OneOf<short, Error> imm16 = ReadS16(ref cur, streamName);
                            if (imm16.IsT1)
                                return imm16.AsT1;

                            RegisterType reg = instruction.Register;
                            Debug.Assert(reg != RegisterType.Unknown);

                            string destination = GetRegisterAssembly(reg);
                            string source = GetValueAssembly(imm16.AsT0, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Or8_RegOrMem_RegOrMem:
                    case OpFamily.Or16_RegOrMem_RegOrMem:
                        {
                            (string destination, string source) = GetDestinationAndSource(modRegRM, destinationIsRegister, isWord, displacement, outputMode);
                            assemblyLine = assemblyLine.WithDestinationAndSource(destination, source);
                        }
                        break;

                    case OpFamily.Or8_FixedReg_Imm:
                    case OpFamily.Or16_FixedReg_Imm:
                        {
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
                            bool specialCase = destinationIsRegister;

                            ArithmeticType atype = (ArithmeticType)modRegRM.RegField;

                            string destination, source;

                            if ((isWord && specialCase) || !isWord)
                            {
                                // 8-bit immediate, but 16-bit register or memory
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

                                // 16-bit immediate and 16-bit register or memory
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

                            string mnemonic = atype switch
                            {
                                ArithmeticType.Add => "ADD",
                                ArithmeticType.AddWithCarry => "ADC",
                                ArithmeticType.SubWithBorrow => "SBB",
                                ArithmeticType.Sub => "SUB",
                                ArithmeticType.Compare => "CMP",
                                _ => throw new NotSupportedException($"Arithmetic type '{atype}' is not supported for instruction '{instruction}'!")
                            };

                            assemblyLine = new AssemblyLine(mnemonic, destination, source);
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
