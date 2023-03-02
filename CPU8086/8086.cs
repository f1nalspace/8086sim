using OneOf;
using System;
using System.Text;

namespace CPU8086
{
    public enum RegisterDirection : byte
    {
        FromRegister = 0,
        ToRegister = 1,
    }

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
        MemoryModeNoDisplacement = 0b00,
        MemoryMode8Bit = 0b01,
        MemoryMode16Bit = 0b10,
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

    public readonly struct OpCode
    {
        public byte Code { get; }
        public string Name { get; }
        public string Description { get; }

        public OpCode(byte code, string name, string description)
        {
            Code = code;
            Name = name;
            Description = description;
        }

        public override string ToString() => $"{Name} [{Code}]";
    }

    public class OpCodeTable
    {
        private readonly OpCode[] _table;

        public ref readonly OpCode this[int index] => ref _table[index];

        public OpCodeTable()
        {
            _table = new OpCode[255];

            _table[136] = new OpCode(0b100010_00, "MOV", "Register/Memory to/from Register");
            _table[96] = new OpCode(0b101100_00, "MOV", "Immediate to Register");
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

    public class CPU
    {
        private readonly OpCodeTable _opTable;
        private readonly RegisterTable _regTable;

        public CPU()
        {
            _opTable = new OpCodeTable();
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

            ReadOnlySpan<byte> cur = stream;
            while (cur.Length >= 2) // Not correct, because there are one byte instructions!
            {
                byte first = cur[0];
                byte opCode = (byte)(first & 0b11111100);
                bool isWide = (first & 0b00000001) == 0b00000001;
                bool directionIsToRegister = (first & 0b00000010) == 0b00000010;

                string firstBinary = first.ToBinary();
                string opCodeBinary = opCode.ToBinary();

                OpCode foundOpCode = _opTable[opCode];
                if (foundOpCode.Code == 0)
                    return new Error(ErrorCode.OpCodeNotImplemented, $"Not implemented opcode '{opCode}'!");
                else if (foundOpCode.Code != opCode)
                    return new Error(ErrorCode.OpCodeMismatch, $"Mismatch opcode! Expect '{opCode}', but got '{foundOpCode.Code}'");

                byte second = cur[1];
                byte mod = (byte)((second >> 6) & 0b11);
                byte reg = (byte)((second >> 3) & 0b111);
                byte rm = (byte)((second >> 0) & 0b111);

                string secondBinary = second.ToBinary();
                string modBinary = mod.ToBinary();
                string regBinary = reg.ToBinary();
                string rmBinary = rm.ToBinary();

                ModeEncoding mode = (ModeEncoding)mod;

                string opCodeName = foundOpCode.Name.ToLower();

                string destination;
                string source;
                switch (mode)
                {
                    case ModeEncoding.RegisterMode:
                        {
                            Register op1;
                            Register op2;
                            if (isWide)
                            {
                                op1 = _regTable.GetWide(reg);
                                op2 = _regTable.GetWide(rm);
                            }
                            else
                            {
                                op1 = _regTable.GetLow(reg);
                                op2 = _regTable.GetLow(rm);
                            }
                            if (directionIsToRegister)
                            {
                                destination = op1.Type.ToString().ToLower();
                                source = op2.Type.ToString().ToLower();
                            }
                            else
                            {
                                source = op1.Type.ToString().ToLower();
                                destination = op2.Type.ToString().ToLower();
                            }
                        }
                        break;
                    default:
                        return new Error(ErrorCode.ModeNotImplemented, $"Not implemented mode '{mode}'");
                }

                s.Append(opCodeName);
                s.Append(' ');
                s.Append(destination);
                s.Append(", ");
                s.Append(source);
                s.AppendLine();

                cur = cur.Slice(2);
            }
            return s.ToString();
        }
    }
}
