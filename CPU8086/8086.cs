using OneOf;
using System;
using System.IO;
using System.Text;

namespace CPU8086
{
    public enum ErrorCode
    {
        Unknown = 0,
        NotEnoughBytesInStream,
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
        }

        public Error(Error error, string message)
        {
            Code = error.Code;
            Message = $"{message}: {error.Message}";
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

    public enum RegisterType
    {
        Unknown = 0,
        AL,
        AX,
        CL,
        CX,
        DL,
        DX,
        BL,
        BX,
        AH,
        SP,
        CH,
        BP,
        DH,
        SI,
        BH,
        DI
    }

    public enum Mode : byte
    {
        MemoryNoDisplacement = 0b00,
        MemoryByteDisplacement = 0b01,
        MemoryWordDisplacement = 0b10,
        RegisterMode = 0b11,
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

        private static string GetName(RegisterType name)
        {
            return name switch
            {
                RegisterType.Unknown => nameof(RegisterType.Unknown),
                RegisterType.AL => nameof(RegisterType.AL),
                RegisterType.AX => nameof(RegisterType.AX),
                RegisterType.CL => nameof(RegisterType.CL),
                RegisterType.CX => nameof(RegisterType.CX),
                RegisterType.DL => nameof(RegisterType.DL),
                RegisterType.DX => nameof(RegisterType.DX),
                RegisterType.BL => nameof(RegisterType.BL),
                RegisterType.BX => nameof(RegisterType.BX),
                RegisterType.AH => nameof(RegisterType.AH),
                RegisterType.SP => nameof(RegisterType.SP),
                RegisterType.CH => nameof(RegisterType.CH),
                RegisterType.BP => nameof(RegisterType.BP),
                RegisterType.DH => nameof(RegisterType.DH),
                RegisterType.SI => nameof(RegisterType.SI),
                RegisterType.BH => nameof(RegisterType.BH),
                RegisterType.DI => nameof(RegisterType.DI),
                _ => null
            };
        }

        public override string ToString() => GetName(Type);
    }

    public enum OpCode : byte
    {
        Unknown = 0,

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
        /// MOV REG8/MEM8, REG8/MEM8
        /// </summary>
        Move8_RegOrMem_Reg,
        /// <summary>
        /// MOV REG8, IMM8 
        /// </summary>
        Move8_Reg_Imm,
        /// <summary>
        /// MOV MEM8, IMM8
        /// </summary>
        Move8_Mem_Imm,
        /// <summary>
        /// MOV REG16/MEM16, REG16/MEM16
        /// </summary>
        Move16_RegOrMem_Reg,
        /// <summary>
        /// MOV REG16, IMM16 
        /// </summary>
        Move16_Reg_Imm,
        /// <summary>
        /// MOV MEM16, IMM16
        /// </summary>
        Move16_Mem_Imm,
        /// <summary>
        /// MOV AL, MEM8
        /// </summary>
        Move8_AL_Mem,
        /// <summary>
        /// MOV AX, MEM16
        /// </summary>
        Move16_AX_Mem,
        /// <summary>
        /// MOV MEM8, AL
        /// </summary>
        Move8_Mem_AL,
        /// <summary>
        /// MOV MEM16, AL
        /// </summary>
        Move16_Mem_AX,
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
        ModRM = 1 << 0,
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

    public class Instruction
    {
        public OpCode OpCode { get; }
        public OpFamily Family { get; }
        public FieldEncoding Encoding { get; }
        public RegisterType Register { get; }
        public byte MinLength { get; }
        public byte MaxLength { get; }
        public string Mnemonic { get; }
        public string Description { get; }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, RegisterType register, byte minLength, byte maxLength, string mnemonic, string description)
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
            if (string.IsNullOrWhiteSpace(mnemonic))
                throw new ArgumentNullException(nameof(mnemonic));
            OpCode = opCode;
            Family = family;
            Encoding = encoding;
            Register = register;
            MinLength = minLength;
            MaxLength = maxLength;
            Mnemonic = mnemonic;
            Description = description;
        }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, byte minLength, byte maxLength, string mnemonic, string description) : this(opCode, family, encoding, RegisterType.Unknown, minLength, maxLength, mnemonic, description)
        {
        }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, RegisterType register, byte minLength, string mnemonic, string description) : this(opCode, family, encoding, register, minLength, minLength, mnemonic, description)
        {
        }

        public Instruction(OpCode opCode, OpFamily family, FieldEncoding encoding, byte minLength, string mnemonic, string description) : this(opCode, family, encoding, RegisterType.Unknown, minLength, minLength, mnemonic, description)
        {
        }

        public override string ToString()
        {
            if (MinLength == MaxLength)
                return $"{OpCode}/{((byte)OpCode).ToBinary()} ({MinLength} bytes, family: {Family}, encoding: {Encoding}, target-register: {Register})";
            else
                return $"{OpCode}/{((byte)OpCode).ToBinary()} ({MinLength} to {MaxLength} bytes, family: {Family}, encoding: {Encoding}, target-register: {Register})";
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

            // 1 0 0 0 1 0 d w
            _table[0x88 /* 100010 00 */] = new Instruction(OpCode.MOV_dREG8_dMEM8_sREG8, OpFamily.Move8_RegOrMem_Reg, FieldEncoding.ModRM, 2, 3, "MOV", "Copy 8-bit Register to 8-bit Register/Memory");
            _table[0x89 /* 100010 01 */] = new Instruction(OpCode.MOV_dREG16_dMEM16_sREG16, OpFamily.Move16_RegOrMem_Reg, FieldEncoding.ModRM, 2, 4, "MOV", "Copy 16-bit Register to 16-bit Register/Memory");
            _table[0x8A /* 100010 10 */] = new Instruction(OpCode.MOV_dREG8_sMEM8_sREG8, OpFamily.Move8_RegOrMem_Reg, FieldEncoding.ModRM, 2, 3, "MOV", "Copy 8-bit Register/Memory to 8-bit Register");
            _table[0x8B /* 100010 11 */] = new Instruction(OpCode.MOV_dREG16_sMEM16_sREG16, OpFamily.Move16_RegOrMem_Reg, FieldEncoding.ModRM, 2, 4, "MOV", "Copy 8-bit Register/Memory to 8-bit Register");

            // 1 0 1 0 0 0 0 0 to 1 0 1 0 0 0 1 1
            _table[0xA0 /* 1010000 */] = new Instruction(OpCode.MOV_dAL_sMEM8, OpFamily.Move8_AL_Mem, FieldEncoding.None, RegisterType.AL, 3, "MOV", "Copy 8-bit Memory to 8-bit " + RegisterType.AL + " Register");
            _table[0xA1 /* 1010001 */] = new Instruction(OpCode.MOV_dAX_sMEM16, OpFamily.Move16_AX_Mem, FieldEncoding.None, RegisterType.AX, 3, "MOV", "Copy 16-bit Memory to 16-bit " + RegisterType.AX + " Register");
            _table[0xA2 /* 1010010 */] = new Instruction(OpCode.MOV_dMEM8_sAL, OpFamily.Move8_Mem_AL, FieldEncoding.None, RegisterType.AL, 3, "MOV", "Copy 8-bit " + RegisterType.AL + " Register to 8-bit Memory");
            _table[0xA3 /* 1010011 */] = new Instruction(OpCode.MOV_dMEM16_sAX, OpFamily.Move16_Mem_AX, FieldEncoding.None, RegisterType.AX, 3, "MOV", "Copy 16-bit " + RegisterType.AX + " Register to 16-bit Memory"); // BUG(final): Page 174, instruction $A3: MOV16, AL is wrong, correct is MOV16, AX

            // 1 0 1 1 w reg
            _table[0xB0 /* 1011 0 000 */] = new Instruction(OpCode.MOV_dAL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AL, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0xB1 /* 1011 0 001 */] = new Instruction(OpCode.MOV_dCL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CL, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.CL + " Register");
            _table[0xB2 /* 1011 0 010 */] = new Instruction(OpCode.MOV_dDL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DL, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.DL + " Register");
            _table[0xB3 /* 1011 0 011 */] = new Instruction(OpCode.MOV_dBL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BL, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.BL + " Register");
            _table[0xB4 /* 1011 0 100 */] = new Instruction(OpCode.MOV_dAH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AH, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.AH + " Register");
            _table[0xB5 /* 1011 0 101 */] = new Instruction(OpCode.MOV_dCH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CH, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.CH + " Register");
            _table[0xB6 /* 1011 0 110 */] = new Instruction(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DH, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.DH + " Register");
            _table[0xB7 /* 1011 0 111 */] = new Instruction(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BH, 2, "MOV", "Copy 8-bit Immediate to 8-bit " + RegisterType.BH + " Register");
            _table[0xB8 /* 1011 1 000 */] = new Instruction(OpCode.MOV_dAX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.AX, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");
            _table[0xB9 /* 1011 1 001 */] = new Instruction(OpCode.MOV_dCX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.CX, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.CX + " Register");
            _table[0xBA /* 1011 1 010 */] = new Instruction(OpCode.MOV_dDX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DX, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.DX + " Register");
            _table[0xBB /* 1011 1 011 */] = new Instruction(OpCode.MOV_dBX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BX, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.BX + " Register");
            _table[0xBC /* 1011 1 100 */] = new Instruction(OpCode.MOV_dSP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SP, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.SP + " Register");
            _table[0xBD /* 1011 1 101 */] = new Instruction(OpCode.MOV_dBP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BP, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.BP + " Register");
            _table[0xBE /* 1011 1 110 */] = new Instruction(OpCode.MOV_dSI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SI, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.SI + " Register");
            _table[0xBF /* 1011 1 111 */] = new Instruction(OpCode.MOV_dDI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DI, 3, "MOV", "Copy 16-bit Immediate to 16-bit " + RegisterType.DI + " Register");

            _table[0xC6 /* 1100 0110 */] = new Instruction(OpCode.MOV_dMEM8_sIMM8, OpFamily.Move8_Mem_Imm, FieldEncoding.ModRM, 3, 5, "MOV", "Copy 8-bit Immediate to 8-bit Memory");
            _table[0xC7 /* 1100 0111 */] = new Instruction(OpCode.MOV_dMEM16_sIMM16, OpFamily.Move16_Mem_Imm, FieldEncoding.ModRM, 4, 6, "MOV", "Copy 16-bit Immediate to 16-bit Memory");
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

        public string GetAssembly(EffectiveAddressCalculation eac, short displacementOrAddress, OutputValueMode outputMode)
        {
            byte displacementLength = _effectiveAddressCalculationTable.GetDisplacementLength(eac);

            string append;
            if (displacementOrAddress == 0)
                append = string.Empty;
            else
            {
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
                string address = GetAssembly(displacementOrAddress, outputMode);
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
                EffectiveAddressCalculation.DirectAddress => $"[{GetAssembly(displacementOrAddress, outputMode)}]",
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

        public static string GetAssembly(byte value, OutputValueMode outputMode) => outputMode switch
        {
            OutputValueMode.AsHexAuto => $"0x{value:X}",
            OutputValueMode.AsHex8 => $"0x{value:X2}",
            OutputValueMode.AsHex16 => $"0x{value:X4}",
            _ => value.ToString(),
        };

        public static string GetAssembly(short value, OutputValueMode outputMode) => outputMode switch
        {
            OutputValueMode.AsHexAuto => $"0x{value:X}",
            OutputValueMode.AsHex8 => $"0x{value:X2}",
            OutputValueMode.AsHex16 => $"0x{value:X4}",
            _ => value.ToString(),
        };

        public static string GetAssembly(RegisterType regType) => regType switch
        {
            RegisterType.AL => "al",
            RegisterType.AX => "ax",
            RegisterType.CL => "cl",
            RegisterType.CX => "cx",
            RegisterType.DL => "dl",
            RegisterType.DX => "dx",
            RegisterType.BL => "bl",
            RegisterType.BX => "bx",
            RegisterType.AH => "ah",
            RegisterType.SP => "sp",
            RegisterType.CH => "ch",
            RegisterType.BP => "bp",
            RegisterType.DH => "dh",
            RegisterType.SI => "si",
            RegisterType.BH => "bh",
            RegisterType.DI => "di",
            _ => null
        };

        public static string GetAssembly(Register reg) => GetAssembly(reg?.Type ?? RegisterType.Unknown);

        public OneOf<string, Error> GetAssembly(Stream stream, string name, OutputValueMode outputMode)
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

        public OneOf<string, Error> GetAssembly(ReadOnlySpan<byte> stream, string streamName, OutputValueMode outputMode)
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
                if (instruction.Encoding.HasFlag(FieldEncoding.ModRM))
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

#if false
                // Get direction is to register
                bool directionIsToRegister = (opCode & 0b00000010) == 0b00000010;

                // Get mod, reg, r/m fields or leave it zero
                Mode mode;
                EffectiveAddressCalculation eac;
                byte modField, regField, rmField;
                if (instruction.Encoding.HasFlag(FieldEncoding.ModRM))
                {
                    modField = (byte)((data[1] >> 6) & 0b111);
                    regField = (byte)((data[1] >> 3) & 0b111);
                    rmField = (byte)((data[1] >> 0) & 0b111);

                    // Translate into effective address calculation
                    mode = (Mode)modField;
                    eac = mode switch
                    {
                        Mode.RegisterMode => EffectiveAddressCalculation.None,
                        _ => _effectiveAddressCalculationTable.Get(rmField, modField)
                    };
                }
                else
                {
                    modField = regField = rmField = 0;
                    mode = Mode.MemoryNoDisplacement;
                    eac = EffectiveAddressCalculation.None;
                }

                // Load additional data for variable length instructions
                if (instruction.MaxLength > instruction.MinLength && mode != Mode.RegisterMode)
                {
                    byte dataLen = eac switch
                    {
                        EffectiveAddressCalculation.BX_SI or
                        EffectiveAddressCalculation.BX_DI or
                        EffectiveAddressCalculation.BP_SI or
                        EffectiveAddressCalculation.BP_DI or
                        EffectiveAddressCalculation.SI or
                        EffectiveAddressCalculation.DI or
                        EffectiveAddressCalculation.BX => 0,

                        EffectiveAddressCalculation.DirectAddress => 2,
                        EffectiveAddressCalculation.BX_SI_D8 or
                        EffectiveAddressCalculation.BX_DI_D8 or
                        EffectiveAddressCalculation.BP_SI_D8 or
                        EffectiveAddressCalculation.BP_DI_D8 => 1,
                        EffectiveAddressCalculation.SI_D8 or
                        EffectiveAddressCalculation.DI_D8 or
                        EffectiveAddressCalculation.BP_D8 or
                        EffectiveAddressCalculation.BX_D8 => 1,

                        EffectiveAddressCalculation.BX_SI_D16 or
                        EffectiveAddressCalculation.BX_DI_D16 or
                        EffectiveAddressCalculation.BP_SI_D16 or
                        EffectiveAddressCalculation.BP_DI_D16 or
                        EffectiveAddressCalculation.SI_D16 or
                        EffectiveAddressCalculation.DI_D16 or
                        EffectiveAddressCalculation.BP_D16 or
                        EffectiveAddressCalculation.BX_D16 => 2,

                        _ => throw new NotImplementedException($"Instruction '{instruction}' is not properly configured"),
                    };
                    if (dataLen > 0)
                    {
                        for (; index < instruction.MinLength + dataLen; ++index)
                            data[index] = cur[index];
                        len += dataLen;
                    }
                }

                short displacement = eac switch
                {
                    // Mod == 01
                    EffectiveAddressCalculation.BX_SI_D8 or
                    EffectiveAddressCalculation.BX_DI_D8 or
                    EffectiveAddressCalculation.BP_SI_D8 or
                    EffectiveAddressCalculation.BP_DI_D8 or
                    EffectiveAddressCalculation.SI_D8 or
                    EffectiveAddressCalculation.DI_D8 or
                    EffectiveAddressCalculation.BP_D8 or
                    EffectiveAddressCalculation.BX_D8 => data[2],

                    // Mod == 10
                    EffectiveAddressCalculation.DirectAddress or
                    EffectiveAddressCalculation.BX_SI_D16 or
                    EffectiveAddressCalculation.BX_DI_D16 or
                    EffectiveAddressCalculation.BP_SI_D16 or
                    EffectiveAddressCalculation.BP_DI_D16 or
                    EffectiveAddressCalculation.SI_D16 or
                    EffectiveAddressCalculation.DI_D16 or
                    EffectiveAddressCalculation.BP_D16 or
                    EffectiveAddressCalculation.BX_D16 => (short)(data[2] | data[3] << 8),

                    _ => 0,
                };
#endif

                string opCodeName = instruction.Mnemonic.ToLower();
                string destination = string.Empty;
                string source = string.Empty;

                switch (instruction.Family)
                {
                    case OpFamily.Move8_RegOrMem_Reg:
                    case OpFamily.Move16_RegOrMem_Reg:
                        {
                            // Get word or byte flag
                            bool isWord = (opCode & 0b00000001) == 0b00000001;

                            // Get direction is to register
                            bool directionIsToRegister = (opCode & 0b00000010) == 0b00000010;

                            if (modRegRM.Mode == Mode.RegisterMode)
                            {
                                if (isWord)
                                {
                                    // 16-bit Register to Register
                                    if (directionIsToRegister)
                                    {
                                        destination = GetAssembly(_regTable.GetWord(modRegRM.RegField));
                                        source = GetAssembly(_regTable.GetWord(modRegRM.RMField));
                                    }
                                    else
                                    {
                                        destination = GetAssembly(_regTable.GetWord(modRegRM.RMField));
                                        source = GetAssembly(_regTable.GetWord(modRegRM.RegField));
                                    }
                                }
                                else
                                {
                                    // 8-bit Register to Register
                                    if (directionIsToRegister)
                                    {
                                        destination = GetAssembly(_regTable.GetByte(modRegRM.RegField));
                                        source = GetAssembly(_regTable.GetByte(modRegRM.RMField));
                                    }
                                    else
                                    {
                                        destination = GetAssembly(_regTable.GetByte(modRegRM.RMField));
                                        source = GetAssembly(_regTable.GetByte(modRegRM.RegField));
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
                                        destination = GetAssembly(_regTable.GetWord(modRegRM.RegField));
                                        source = GetAssembly(modRegRM.EAC, displacement, outputMode);
                                    }
                                    else
                                    {
                                        // 16-bit Register to Memory
                                        destination = GetAssembly(modRegRM.EAC, displacement, outputMode);
                                        source = GetAssembly(_regTable.GetWord(modRegRM.RegField));
                                    }
                                }
                                else
                                {
                                    if (directionIsToRegister)
                                    {
                                        // 8-bit Memory to Register
                                        destination = GetAssembly(_regTable.GetByte(modRegRM.RegField));
                                        source = GetAssembly(modRegRM.EAC, displacement, outputMode);
                                    }
                                    else
                                    {
                                        // 8-bit Register to Memory
                                        destination = GetAssembly(modRegRM.EAC, displacement, outputMode);
                                        source = GetAssembly(_regTable.GetByte(modRegRM.RegField));
                                    }
                                }
                            }
                        }
                        break;

                    case OpFamily.Move8_Reg_Imm:
                        {
                            // 8-bit Immediate To Register
                            OneOf<byte, Error> imm8 = ReadU8(ref cur, streamName);
                            if (imm8.IsT1)
                                return imm8.AsT1;
                            byte reg = (byte)(opCode & 0b00000111);
                            destination = GetAssembly(_regTable.GetByte(reg));
                            source = GetAssembly(imm8.AsT0, outputMode);
                        }
                        break;
                    case OpFamily.Move16_Reg_Imm:
                        {
                            // 16-bit Immediate To Register
                            OneOf<short, Error> imm16 = ReadS16(ref cur, streamName);
                            if (imm16.IsT1)
                                return imm16.AsT1;
                            byte reg = (byte)(opCode & 0b00000111);
                            destination = GetAssembly(_regTable.GetWord(reg));
                            source = GetAssembly(imm16.AsT0, outputMode);
                        }
                        break;
                    case OpFamily.Move8_Mem_Imm:
                        {
                            // 8-bit Explicit Immediate to Memory
                            OneOf<byte, Error> imm8 = ReadU8(ref cur, streamName);
                            if (imm8.IsT1)
                                return imm8.AsT1;
                            source = $"byte {GetAssembly(imm8.AsT0, outputMode)}";
                            destination = GetAssembly(modRegRM.EAC, displacement, outputMode);
                        }
                        break;
                    case OpFamily.Move16_Mem_Imm:
                        {
                            // 16-bit Explicit Immediate to Memory
                            OneOf<short, Error> imm16 = ReadS16(ref cur, streamName);
                            if (imm16.IsT1)
                                return imm16.AsT1;
                            source = $"word {GetAssembly(imm16.AsT0, outputMode)}";
                            destination = GetAssembly(modRegRM.EAC, displacement, outputMode);
                        }
                        break;
                    case OpFamily.Move8_AL_Mem:
                        {
                            OneOf<short, Error> mem16 = ReadS16(ref cur, streamName);
                            if (mem16.IsT1)
                                return mem16.AsT1;
                            source = GetAssembly(EffectiveAddressCalculation.DirectAddress, mem16.AsT0, outputMode);
                            destination = GetAssembly(RegisterType.AL);
                        }
                        break;
                    case OpFamily.Move16_AX_Mem:
                        {
                            OneOf<short, Error> mem16 = ReadS16(ref cur, streamName);
                            if (mem16.IsT1)
                                return mem16.AsT1;
                            source = GetAssembly(EffectiveAddressCalculation.DirectAddress, mem16.AsT0, outputMode);
                            destination = GetAssembly(RegisterType.AX);
                        }
                        break;
                    case OpFamily.Move8_Mem_AL:
                        {
                            OneOf<short, Error> mem16 = ReadS16(ref cur, streamName);
                            if (mem16.IsT1)
                                return mem16.AsT1;
                            destination = GetAssembly(EffectiveAddressCalculation.DirectAddress, mem16.AsT0, outputMode);
                            source = GetAssembly(RegisterType.AL);
                        }
                        break;
                    case OpFamily.Move16_Mem_AX:
                        {
                            OneOf<short, Error> mem16 = ReadS16(ref cur, streamName);
                            if (mem16.IsT1)
                                return mem16.AsT1;
                            destination = GetAssembly(EffectiveAddressCalculation.DirectAddress, mem16.AsT0, outputMode);
                            source = GetAssembly(RegisterType.AX);
                        }
                        break;
                    case OpFamily.Unknown:
                    default:
                        return new Error(ErrorCode.InstructionNotImplemented, $"Not implemented instruction '{instruction}'!");
                }

                StringBuilder line = new StringBuilder();
                line.Append(opCodeName);
                line.Append(' ');
                line.Append(destination.ToLower());
                line.Append(", ");
                line.Append(source.ToLower());

                s.AppendLine(line.ToString());

                if (cur.Length >= start.Length)
                    throw new InvalidOperationException($"Stream '{streamName}' was not properly advanced!");
            }
            return s.ToString();
        }
    }
}
