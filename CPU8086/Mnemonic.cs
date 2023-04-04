using System;

namespace Final.CPU8086
{
    public readonly struct Mnemonic
    {
        public InstructionType Type { get; }

        public Mnemonic(InstructionType type)
        {
            Type = type;
        }

        public Mnemonic(string name)
        {
            Type = NameToType(name);
        }

        public static Mnemonic Parse(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (!TryParse(name, out Mnemonic result))
                throw new NotSupportedException($"The mnemonic '{name}' is not supported!");
            return result;
        }

        private static InstructionType NameToType(string name)
        {
            InstructionType result = (name ?? string.Empty) switch
            {
                "AAA" => InstructionType.AAA,
                "AAD" => InstructionType.AAD,
                "AAM" => InstructionType.AAM,
                "AAS" => InstructionType.AAS,
                "ADC" => InstructionType.ADC,
                "ADD" => InstructionType.ADD,
                "AND" => InstructionType.AND,
                "CALL" => InstructionType.CALL,
                "CBW" => InstructionType.CBW,
                "CLC" => InstructionType.CLC,
                "CLD" => InstructionType.CLD,
                "CLI" => InstructionType.CLI,
                "CMC" => InstructionType.CMC,
                "CMP" => InstructionType.CMP,
                "CMPSB" => InstructionType.CMPSB,
                "CMPSW" => InstructionType.CMPSW,
                "CWD" => InstructionType.CWD,
                "DAA" => InstructionType.DAA,
                "DAS" => InstructionType.DAS,
                "DEC" => InstructionType.DEC,
                "DIV" => InstructionType.DIV,
                "HLT" => InstructionType.HLT,
                "IDIV" => InstructionType.IDIV,
                "IMUL" => InstructionType.IMUL,
                "IN" => InstructionType.IN,
                "INC" => InstructionType.INC,
                "INT" => InstructionType.INT,
                "INTO" => InstructionType.INTO,
                "IRET" => InstructionType.IRET,
                "JA" => InstructionType.JA,
                "JAE" => InstructionType.JAE,
                "JB" => InstructionType.JB,
                "JBE" => InstructionType.JBE,
                "JC" => InstructionType.JC,
                "JCXZ" => InstructionType.JCXZ,
                "JE" => InstructionType.JE,
                "JG" => InstructionType.JG,
                "JGE" => InstructionType.JGE,
                "JL" => InstructionType.JL,
                "JLE" => InstructionType.JLE,
                "JMP" => InstructionType.JMP,
                "JNC" => InstructionType.JNC,
                "JNE" => InstructionType.JNE,
                "JNO" => InstructionType.JNO,
                "JNS" => InstructionType.JNS,
                "JNP" => InstructionType.JNP,
                "JO" => InstructionType.JO,
                "JP" => InstructionType.JP,
                "JS" => InstructionType.JS,
                "LAHF" => InstructionType.LAHF,
                "LDS" => InstructionType.LDS,
                "LEA" => InstructionType.LEA,
                "LES" => InstructionType.LES,
                "LOCK" => InstructionType.LOCK,
                "LODSB" => InstructionType.LODSB,
                "LODSW" => InstructionType.LODSW,
                "LOOP" => InstructionType.LOOP,
                "LOOPE" => InstructionType.LOOPE,
                "LOOPNZ" => InstructionType.LOOPNZ,
                "MOV" => InstructionType.MOV,
                "MOVSB" => InstructionType.MOVSB,
                "MOVSW" => InstructionType.MOVSW,
                "MUL" => InstructionType.MUL,
                "NEG" => InstructionType.NEG,
                "NOP" => InstructionType.NOP,
                "NOT" => InstructionType.NOT,
                "OR" => InstructionType.OR,
                "OUT" => InstructionType.OUT,
                "POP" => InstructionType.POP,
                "POPF" => InstructionType.POPF,
                "PUSH" => InstructionType.PUSH,
                "PUSHF" => InstructionType.PUSHF,
                "RCL" => InstructionType.RCL,
                "RCR" => InstructionType.RCR,
                "REP" => InstructionType.REP,
                "REPE" => InstructionType.REPE,
                "REPNE" => InstructionType.REPNE,
                "RET" => InstructionType.RET,
                "RETF" => InstructionType.RETF,
                "ROL" => InstructionType.ROL,
                "ROR" => InstructionType.ROR,
                "SAHF" => InstructionType.SAHF,
                "SAL" => InstructionType.SAL,
                "SHL" => InstructionType.SHL,
                "SAR" => InstructionType.SAR,
                "SBB" => InstructionType.SBB,
                "SCASB" => InstructionType.SCASB,
                "SCASW" => InstructionType.SCASW,
                "SHR" => InstructionType.SHR,
                "STC" => InstructionType.STC,
                "STD" => InstructionType.STD,
                "STI" => InstructionType.STI,
                "STOSB" => InstructionType.STOSB,
                "STOSW" => InstructionType.STOSW,
                "SUB" => InstructionType.SUB,
                "TEST" => InstructionType.TEST,
                "WAIT" => InstructionType.WAIT,
                "XCHG" => InstructionType.XCHG,
                "XLAT" => InstructionType.XLAT,
                "XOR" => InstructionType.XOR,
                _ => InstructionType.None,
            };
            return result;
        }

        public static bool TryParse(string name, out Mnemonic result)
        {
            InstructionType type = NameToType(name);
            result = new Mnemonic(type);
            return result.Type != InstructionType.None;
        }

        public static implicit operator Mnemonic(InstructionType type) => new Mnemonic(type);
        public static implicit operator Mnemonic(string value) => Parse(value);
        public static explicit operator string(Mnemonic name) => name.ToString();

        public override string ToString()
        {
            return Type switch
            {
                InstructionType.AAA => "AAA",
                InstructionType.AAD => "AAD",
                InstructionType.AAM => "AAM",
                InstructionType.AAS => "AAS",
                InstructionType.ADC => "ADC",
                InstructionType.ADD => "ADD",
                InstructionType.AND => "AND",
                InstructionType.CALL => "CALL",
                InstructionType.CBW => "CBW",
                InstructionType.CLC => "CLC",
                InstructionType.CLD => "CLD",
                InstructionType.CLI => "CLI",
                InstructionType.CMC => "CMC",
                InstructionType.CMP => "CMP",
                InstructionType.CMPSB => "CMPSB",
                InstructionType.CMPSW => "CMPSW",
                InstructionType.CWD => "CWD",
                InstructionType.DAA => "DAA",
                InstructionType.DAS => "DAS",
                InstructionType.DEC => "DEC",
                InstructionType.DIV => "DIV",
                InstructionType.HLT => "HLT",
                InstructionType.IDIV => "IDIV",
                InstructionType.IMUL => "IMUL",
                InstructionType.IN => "IN",
                InstructionType.INC => "INC",
                InstructionType.INT => "INT",
                InstructionType.INTO => "INTO",
                InstructionType.IRET => "IRET",
                InstructionType.JA => "JA",
                InstructionType.JAE => "JAE",
                InstructionType.JB => "JB",
                InstructionType.JBE => "JBE",
                InstructionType.JC => "JC",
                InstructionType.JCXZ => "JCXZ",
                InstructionType.JE => "JE",
                InstructionType.JG => "JG",
                InstructionType.JGE => "JGE",
                InstructionType.JL => "JL",
                InstructionType.JLE => "JLE",
                InstructionType.JMP => "JMP",
                InstructionType.JNC => "JNC",
                InstructionType.JNE => "JNE",
                InstructionType.JNO => "JNO",
                InstructionType.JNS => "JNS",
                InstructionType.JNP => "JNP",
                InstructionType.JO => "JO",
                InstructionType.JP => "JP",
                InstructionType.JS => "JS",
                InstructionType.LAHF => "LAHF",
                InstructionType.LDS => "LDS",
                InstructionType.LEA => "LEA",
                InstructionType.LES => "LES",
                InstructionType.LOCK => "LOCK",
                InstructionType.LODSB => "LODSB",
                InstructionType.LODSW => "LODSW",
                InstructionType.LOOP => "LOOP",
                InstructionType.LOOPE => "LOOPE",
                InstructionType.LOOPNZ => "LOOPNZ",
                InstructionType.MOV => "MOV",
                InstructionType.MOVSB => "MOVSB",
                InstructionType.MOVSW => "MOVSW",
                InstructionType.MUL => "MUL",
                InstructionType.NEG => "NEG",
                InstructionType.NOP => "NOP",
                InstructionType.NOT => "NOT",
                InstructionType.OR => "OR",
                InstructionType.OUT => "OUT",
                InstructionType.POP => "POP",
                InstructionType.POPF => "POPF",
                InstructionType.PUSH => "PUSH",
                InstructionType.PUSHF => "PUSHF",
                InstructionType.RCL => "RCL",
                InstructionType.RCR => "RCR",
                InstructionType.REP => "REP",
                InstructionType.REPE => "REPE",
                InstructionType.REPNE => "REPNE",
                InstructionType.RET => "RET",
                InstructionType.RETF => "RETF",
                InstructionType.ROL => "ROL",
                InstructionType.ROR => "ROR",
                InstructionType.SAHF => "SAHF",
                InstructionType.SAL => "SAL",
                InstructionType.SHL => "SHL",
                InstructionType.SAR => "SAR",
                InstructionType.SBB => "SBB",
                InstructionType.SCASB => "SCASB",
                InstructionType.SCASW => "SCASW",
                InstructionType.SHR => "SHR",
                InstructionType.STC => "STC",
                InstructionType.STD => "STD",
                InstructionType.STI => "STI",
                InstructionType.STOSB => "STOSB",
                InstructionType.STOSW => "STOSW",
                InstructionType.SUB => "SUB",
                InstructionType.TEST => "TEST",
                InstructionType.WAIT => "WAIT",
                InstructionType.XCHG => "XCHG",
                InstructionType.XLAT => "XLAT",
                InstructionType.XOR => "XOR",
                _ => string.Empty,
            };
        }
    }
}
