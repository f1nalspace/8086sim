using OneOf;
using System;
using System.Diagnostics;
using System.Text;

namespace CPU8086
{
    public enum RegisterType
    {
        None = 0,
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

    public enum ModeEncoding : byte
    {
        MemoryNoDisplacement = 0b00,
        MemoryByteDisplacement = 0b01,
        MemoryWordDisplacement = 0b10,
        RegisterMode = 0b11,
    }

    public readonly struct Register
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
                RegisterType.None => nameof(RegisterType.None),
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
        None = 0,
        MOV_R8_R8 = 0x88,
        MOV_R16_R16 = 0x89,
        MOV_AX_IMM16 = 0xB8,
    }

    public readonly struct Instruction
    {
        public OpCode OpCode { get; }
        public byte Length { get; }
        public string Mnemonic { get; }
        public string Description { get; }

        public Instruction(OpCode opCode, byte length, string mnemonic, string description)
        {
            OpCode = opCode;
            Length = length;
            Mnemonic = mnemonic;
            Description = description;
        }

        public override string ToString() => $"{Mnemonic} ({Length} bytes) [{OpCode}]";
    }

    public class InstructionTable
    {
        private readonly Instruction[] _table;

        public ref readonly Instruction this[int index] => ref _table[index];

        public InstructionTable()
        {
            _table = new Instruction[256];

            _table[0x88] = new Instruction(OpCode.MOV_R8_R8, 2, "MOV", "8-bit Register to 8-bit Register");
            _table[0x89] = new Instruction(OpCode.MOV_R16_R16, 2, "MOV", "16-bit Register to 16-bit Register");
            _table[0xB8] = new Instruction(OpCode.MOV_AX_IMM16, 3, "MOV", "16-bit Immediate to AX Register");
        }
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

        public ref readonly Register GetLow(byte index) => ref _table[index];
        public ref readonly Register GetWide(byte index) => ref _table[index + 8];
    }

    public enum ErrorCode
    {
        Unknown = 0,
        UnexpectedEndOfStream,
        OpCodeNotImplemented,
        OpCodeMismatch,
        InstructionNotImplemented,
        InstructionTooLong,
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

        public override string ToString() => $"[{Code}] {Message}";
    }

    public class CPU
    {
        private readonly InstructionTable _opTable;
        private readonly RegisterTable _regTable;

        public CPU()
        {
            _opTable = new InstructionTable();
            _regTable = new RegisterTable();
        }

        public OneOf<string, Error> GetAssembly(ReadOnlySpan<byte> stream, string streamName)
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

            Span<byte> data = stackalloc byte[6];

            ReadOnlySpan<byte> cur = stream;
            while (cur.Length > 0)
            {
                byte opCode = data[0] = cur[0];

                Instruction instruction = _opTable[opCode];
                if (instruction.OpCode == OpCode.None)
                    return new Error(ErrorCode.OpCodeNotImplemented, $"Not implemented opcode '${opCode:X2}'!");
                else if ((byte)instruction.OpCode != opCode)
                    return new Error(ErrorCode.OpCodeMismatch, $"Mismatch opcode! Expect '${opCode:X2}', but got '{instruction.OpCode}'");
                if (instruction.Length > 6)
                    return new Error(ErrorCode.InstructionTooLong, $"Instruction '{instruction}' is too long, only 6 bytes are allowed!");
                if (cur.Length < instruction.Length)
                    return new Error(ErrorCode.UnexpectedEndOfStream, $"The stream has not enough bytes for the instruction '{instruction}, expect {instruction.Length} bytes, but got left {cur.Length}'!");

                byte index = 1;

                // Load data for instruction
                for (index = 1; index < instruction.Length; ++index)
                    data[index] = cur[index];

                // Clear remaining data to zero
                for (; index < 6; ++index)
                    data[index] = 0; 

                string opCodeName = instruction.Mnemonic.ToLower();
                string destination = string.Empty;
                string source = string.Empty;

                byte mod = (byte)((data[1] >> 6) & 0b111);
                byte reg = (byte)((data[1] >> 3) & 0b111);
                byte rm = (byte)((data[1] >> 0) & 0b111);

                switch (instruction.OpCode)
                {
                    case OpCode.MOV_R8_R8:
                        {
                            ref readonly Register destReg = ref _regTable.GetLow(rm);
                            ref readonly Register sourceReg = ref _regTable.GetLow(reg);
                            destination = destReg.Type.ToString();
                            source = sourceReg.Type.ToString();
                        }
                        break;
                    case OpCode.MOV_R16_R16:
                        {
                            ref readonly Register destReg = ref _regTable.GetWide(rm);
                            ref readonly Register sourceReg = ref _regTable.GetWide(reg);
                            destination = destReg.Type.ToString();
                            source = sourceReg.Type.ToString();
                        }
                        break;
                    case OpCode.MOV_AX_IMM16:
                        {
                            byte low = data[1];
                            byte high = data[2];
                            int value = low | high << 8;
                            destination = RegisterType.AX.ToString();
                            source = $"0{value:X}h";
                        }
                        break;
                    default:
                        return new Error(ErrorCode.InstructionNotImplemented, $"Not implemented instruction for opcode '{instruction.OpCode}'!");
                }

                s.Append(opCodeName);
                s.Append(' ');
                s.Append(destination.ToLower());
                s.Append(", ");
                s.Append(source.ToLower());
                s.AppendLine();

                cur = cur.Slice(instruction.Length);
            }
            return s.ToString();
        }
    }
}
