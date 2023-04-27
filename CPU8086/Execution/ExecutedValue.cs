using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using System.Runtime.InteropServices;

namespace Final.CPU8086.Execution
{
    public enum ExecutedValueType : byte
    {
        None = 0,
        Register,
        Memory,
        Flags,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct ExecutedValue
    {
        [FieldOffset(0)]
        public readonly ExecutedValueType Type;
        [FieldOffset(1)]
        public readonly RegisterType Register;
        [FieldOffset(1)]
        public readonly MemoryAddress Memory;
        [FieldOffset(1)]
        public readonly Immediate Value;
        [FieldOffset(1)]
        public readonly Flags Flags;

        public ExecutedValue(RegisterType reg, Immediate value)
        {
            Flags = new Flags();
            Memory = new MemoryAddress();
            Type = ExecutedValueType.Register;
            Register = reg;
            Value = value;
        }

        public ExecutedValue(MemoryAddress memory, Immediate value)
        {
            Flags = new Flags();
            Register = RegisterType.Unknown;
            Type = ExecutedValueType.Memory;
            Memory = memory;
            Value = value;
        }

        public ExecutedValue(Flags flags)
        {
            Memory = new MemoryAddress();
            Value = new Immediate();
            Register = RegisterType.Unknown;
            Flags = flags;
            Type = ExecutedValueType.Memory;
        }
    }
}
