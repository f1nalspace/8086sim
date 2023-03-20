using System;

namespace Final.CPU8086
{
    public class InstructionDefinition
    {
        public OpCode OpCode { get; }
        public OpFamily Family { get; }
        public FieldEncoding Encoding { get; }
        public RegisterType Register { get; }
        public byte MinLength { get; }
        public byte MaxLength { get; }
        public Mnemonic Mnemonic { get; }
        public string Description { get; }

        public InstructionDefinition(OpCode opCode, OpFamily family, FieldEncoding encoding, RegisterType register, byte minLength, byte maxLength, Mnemonic mnemonic, string description)
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

        public InstructionDefinition(OpCode opCode, OpFamily family, FieldEncoding encoding, byte minLength, byte maxLength, Mnemonic mnemonic, string description) : this(opCode, family, encoding, RegisterType.Unknown, minLength, maxLength, mnemonic, description)
        {
        }

        public InstructionDefinition(OpCode opCode, OpFamily family, FieldEncoding encoding, RegisterType register, byte minLength, Mnemonic mnemonic, string description) : this(opCode, family, encoding, register, minLength, minLength, mnemonic, description)
        {
        }

        public InstructionDefinition(OpCode opCode, OpFamily family, FieldEncoding encoding, byte minLength, Mnemonic mnemonic, string description) : this(opCode, family, encoding, RegisterType.Unknown, minLength, minLength, mnemonic, description)
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
}
