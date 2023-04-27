using Final.CPU8086.Instructions;
using Final.CPU8086.Types;

namespace Final.CPU8086.Execution
{
    public readonly struct ExecutedValue
    {
        public OperandType Type { get; }
        public RegisterType Register { get; }
        public MemoryAddress Memory { get; }
        public Immediate Value { get; }

        public ExecutedValue(RegisterType reg, Immediate value)
        {
            Memory = new MemoryAddress();
            Type = OperandType.Register;
            Register = reg;
            Value = value;
        }

        public ExecutedValue(MemoryAddress memory, Immediate value)
        {
            Register = RegisterType.Unknown;
            Type = OperandType.Address;
            Memory = memory;
            Value = value;
        }
    }
}
