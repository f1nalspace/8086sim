using System;

namespace Final.CPUEmulation
{
    public class Instruction
    {
        public Mnemonic Mnemonic { get; }
        public DataWidth Width { get; }
        public Operand Destination { get; }
        public Operand Source { get; }
        public uint OpCode { get; }
        public uint Position { get; }
        public uint Flags { get; }
        public byte Length { get; }

        public Instruction(uint opCode, uint position, Mnemonic mnemonic, DataWidth width, uint flags, byte length, Operand? destination = null, Operand? source = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(mnemonic));
            OpCode = opCode;
            Position = position;
            Mnemonic = mnemonic;
            Width = width;
            Flags = flags;
            Length = length;
            Destination = destination ?? new Operand();
            Source = source ?? new Operand();
        }

        public override string ToString() => $"{Mnemonic.Name} / Op: {OpCode:X} / Len: {Length} bytes";
    }
}
