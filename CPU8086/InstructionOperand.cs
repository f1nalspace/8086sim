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

        private InstructionOperand(RegisterType register)
        {
            Type = OperandType.Register;
            Memory = new MemoryAddress();
            Immediate = new Immediate();
            Register = register;
        }

        private InstructionOperand(Immediate immediate)
        {
            Type = OperandType.Register;
            Memory = new MemoryAddress();
            Register = RegisterType.Unknown;
            Immediate = immediate;
        }

        private InstructionOperand(MemoryAddress address)
        {
            Type = OperandType.Address;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Memory = address;
        }

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

        public static InstructionOperand AsImmediate(short immediate, ImmediateFlag flags = ImmediateFlag.None)
            => new InstructionOperand(new Immediate(immediate, flags));

        public static InstructionOperand AsAddress(EffectiveAddressCalculation eac, short address)
            => new InstructionOperand(new MemoryAddress(eac, address));

        public static InstructionOperand AsRegister(RegisterType reg)
            => new InstructionOperand(reg);

        public static InstructionOperand AsRegister(Register reg)
            => AsRegister(reg?.Type ?? RegisterType.Unknown);
    }
}
