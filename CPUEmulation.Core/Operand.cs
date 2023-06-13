using System.Runtime.InteropServices;

namespace Final.CPUEmulation
{
    public enum OperandType : byte
    {
        None = 0,
        Register,
        Immediate,
        Memory,
    }

    [StructLayout(LayoutKind.Explicit,  Pack = 1)]
    public readonly struct Operand
    {
        [FieldOffset(0)]
        public readonly OperandType Type;
        [FieldOffset(1)]
        public readonly DataType DataType;
        [FieldOffset(3)]
        public readonly MemoryAddress Mem;
        [FieldOffset(3)]
        public readonly Immediate Imm;
        [FieldOffset(3)]
        public readonly Register Reg;

        public Operand(DataType dataType, MemoryAddress memory)
        {
            Type = OperandType.Memory;
            DataType = dataType;
            Imm = new Immediate();
            Reg = new Register();
            Mem = memory;
        }

        public Operand(Immediate immediate)
        {
            Type = OperandType.Immediate;
            DataType = immediate.Type switch
            {
                ImmediateType.U8 => DataType.U8,
                ImmediateType.S8 => DataType.S8,
                ImmediateType.U16 => DataType.U16,
                ImmediateType.S16 => DataType.S16,
                ImmediateType.U32 => DataType.U32,
                ImmediateType.S32 => DataType.S32,
                _ => new DataType(),
            };
            Mem = new MemoryAddress();
            Reg = new Register();
            Imm = immediate;
        }

        public Operand(Register reg)
        {
            Type = OperandType.Register;
            DataType = reg.Width.Type switch
            {
                DataWidthType.Byte => DataType.U8,
                DataWidthType.Word => DataType.U16,
                DataWidthType.DoubleWord => DataType.U32,
                DataWidthType.QuadWord => DataType.U64,
                _ => new DataType(),
            };
            Mem = new MemoryAddress();
            Imm = new Immediate();
            Reg = reg;
        }
    }
}
