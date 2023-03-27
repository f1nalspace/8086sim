using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct InstructionOperand
    {
        [FieldOffset(0)]
        public readonly OperandType Type;
        [FieldOffset(1)]
        public readonly MemoryAddress Memory;
        [FieldOffset(1)]
        public readonly RegisterType Register;
        [FieldOffset(1)]
        public readonly Immediate Immediate;

        public InstructionOperand(RegisterType register)
        {
            Type = OperandType.Register;
            Memory = new MemoryAddress();
            Immediate = new Immediate();
            Register = register;
        }

        public InstructionOperand(Register register)
            : this(register?.Type ?? RegisterType.Unknown) { }

        public InstructionOperand(Immediate immediate)
        {
            Type = OperandType.Register;
            Memory = new MemoryAddress();
            Register = RegisterType.Unknown;
            Immediate = immediate;
        }

        public InstructionOperand(byte imm8, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(imm8, flags)) { }

        public InstructionOperand(short imm16, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(imm16, flags)) { }

        public InstructionOperand(MemoryAddress address)
        {
            Type = OperandType.Address;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Memory = address;
        }

        public InstructionOperand(EffectiveAddressCalculation eac, short displacement)
            : this(new MemoryAddress(eac, displacement)) { }

        public override string ToString()
        {
            return Type switch
            {
                OperandType.Register => CPU8086.Register.GetName(Register),
                OperandType.Immediate => Immediate.ToString(),
                OperandType.Address => Memory.ToString(),
                _ => "None"
            };
        }
    }
}
