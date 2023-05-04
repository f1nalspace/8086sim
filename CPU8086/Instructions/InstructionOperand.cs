using System;
using System.Runtime.InteropServices;
using Final.CPU8086.Types;

namespace Final.CPU8086.Instructions
{
    public enum OperandType : byte
    {
        None = 0,
        Register,
        Accumulator,
        Segment,
        Memory,
        Immediate,
        Value,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct InstructionOperand : IEquatable<InstructionOperand>
    {
        [FieldOffset(0)]
        public readonly OperandType Type;
        [FieldOffset(1)]
        public readonly DataType DataType;
        [FieldOffset(2)]
        public readonly MemoryAddress Memory;
        [FieldOffset(2)]
        public readonly RegisterType Register;
        [FieldOffset(2)]
        public readonly Immediate Immediate;
        [FieldOffset(2)]
        public readonly int Value;

        public InstructionOperand(RegisterType register, DataType dataType = DataType.None)
        {
            Type = Types.Register.IsAccumulator(register) ? OperandType.Accumulator : Types.Register.IsSegment(register) ? OperandType.Segment : OperandType.Register;
            DataType = dataType != DataType.None ? dataType : Types.Register.GetDataType(register);
            Memory = new MemoryAddress();
            Immediate = new Immediate();
            Value = 0;
            Register = register;
        }

        public InstructionOperand(Register register, DataType dataType)
            : this(register?.Type ?? RegisterType.Unknown, dataType) { }

        public InstructionOperand(Immediate immediate)
        {
            Type = OperandType.Immediate;
            DataType = immediate.Type switch
            {
                ImmediateType.Byte or ImmediateType.SignedByte => DataType.Byte,
                ImmediateType.Word or ImmediateType.SignedWord => DataType.Word,
                ImmediateType.DoubleWord or ImmediateType.Int => DataType.DoubleWord,
                _ => DataType.None,
            };
            Memory = new MemoryAddress();
            Register = RegisterType.Unknown;
            Value = 0;
            Immediate = immediate;
        }

        public InstructionOperand(byte u8, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(u8, flags)) { }

        public InstructionOperand(sbyte s8, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(s8, flags)) { }

        public InstructionOperand(short s16, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(s16, flags)) { }

        public InstructionOperand(ushort u16, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(u16, flags)) { }

        public InstructionOperand(uint u32, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(u32, flags)) { }

        public InstructionOperand(int s32, ImmediateFlag flags = ImmediateFlag.None)
            : this(new Immediate(s32, flags)) { }

        public InstructionOperand(MemoryAddress address, DataType dataType)
        {
            Type = OperandType.Memory;
            DataType = dataType;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Value = 0;
            Memory = address;
        }

        public InstructionOperand(int value, DataType dataType)
        {
            Type = OperandType.Memory;
            DataType = dataType;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Memory = new MemoryAddress();
            Value = value;
        }

        public bool Equals(InstructionOperand other)
        {
            if (Type != other.Type) return false;
            if (DataType != other.DataType) return false;
            switch (Type)
            {
                case OperandType.Register:
                case OperandType.Accumulator:
                case OperandType.Segment:
                    if (!Register.Equals(other.Register))
                        return false;
                    break;
                case OperandType.Memory:
                    if (!Memory.Equals(other.Memory))
                        return false;
                    break;
                case OperandType.Immediate:
                    if (!Immediate.Equals(other.Immediate))
                        return false;
                    break;
                case OperandType.Value:
                    if (Value != other.Value)
                        return false;
                    break;
            }
            return true;
        }
        public override bool Equals(object obj) => obj is InstructionOperand op && Equals(op);
        public override int GetHashCode()
        {
            return Type switch
            {
                OperandType.Register or
                OperandType.Accumulator or
                OperandType.Segment => HashCode.Combine(Type, DataType, Register),
                OperandType.Memory => HashCode.Combine(Type, DataType, Memory),
                OperandType.Immediate => HashCode.Combine(Type, DataType, Immediate),
                OperandType.Value => HashCode.Combine(Type, DataType, Value),
                _ => HashCode.Combine(Type, DataType),
            };
        }



        public override string ToString()
        {
            return Type switch
            {
                OperandType.Register => $"Reg[{DataType}]: {Types.Register.GetName(Register)}",
                OperandType.Accumulator => $"Acc[{DataType}]: {Types.Register.GetName(Register)}",
                OperandType.Segment => $"Seg[{DataType}]: {Types.Register.GetName(Register)}",
                OperandType.Immediate => $"Imm[{DataType}]: {Immediate}",
                OperandType.Memory => $"Mem[{DataType}]: {Memory}",
                OperandType.Value => $"Value[{DataType}]: {Value}",
                _ => string.Empty
            };
        }
    }
}
