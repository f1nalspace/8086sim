using System;
using System.Runtime.InteropServices;

namespace Final.CPU8086
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct InstructionOperand : IEquatable<InstructionOperand>
    {
        [FieldOffset(0)]
        public readonly OperandType Op;
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
            Op = OperandType.Register;
            DataType = dataType;
            Memory = new MemoryAddress();
            Immediate = new Immediate();
            Value = 0;
            Register = register;
        }

        public InstructionOperand(Register register, DataType dataType = DataType.None)
            : this(register?.Type ?? RegisterType.Unknown, dataType) { }

        public InstructionOperand(Immediate immediate, DataType dataType = DataType.None)
        {
            Op = OperandType.Immediate;
            DataType = dataType;
            Memory = new MemoryAddress();
            Register = RegisterType.Unknown;
            Value = 0;
            Immediate = immediate;
        }

        public InstructionOperand(byte imm8, ImmediateFlag flags = ImmediateFlag.None, DataType dataType = DataType.None)
            : this(new Immediate(imm8, flags), dataType) { }

        public InstructionOperand(short imm16, ImmediateFlag flags = ImmediateFlag.None, DataType dataType = DataType.None)
            : this(new Immediate(imm16, flags), dataType) { }

        public InstructionOperand(MemoryAddress address, DataType dataType = DataType.None)
        {
            Op = OperandType.Address;
            DataType = dataType;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Value = 0;
            Memory = address;
        }

        public InstructionOperand(int value, DataType dataType = DataType.None)
        {
            Op = OperandType.Address;
            DataType = dataType;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Memory = new MemoryAddress();
            Value = value;
        }

        public InstructionOperand(EffectiveAddressCalculation eac, int displacement, DataType dataType = DataType.None)
            : this(new MemoryAddress(eac, displacement), dataType) { }

        public bool Equals(InstructionOperand other)
        {
            if (Op != other.Op) return false;
            if (DataType != other.DataType) return false;
            switch (Op)
            {
                case OperandType.Register:
                    if (!Register.Equals( other.Register))
                        return false;
                    break;
                case OperandType.Address:
                    if (!Memory.Equals(other.Memory))
                        return false;
                    break;
                case OperandType.Immediate:
                    if (!Immediate.Equals(other.Immediate))
                        return false; break;
            }
            return true;
        }
        public override bool Equals(object obj) => obj is InstructionOperand op && Equals(op);
        public override int GetHashCode()
        {
            return Op switch
            {
                OperandType.Register => HashCode.Combine(Op, DataType, Register),
                OperandType.Address => HashCode.Combine(Op, DataType, Memory),
                OperandType.Immediate => HashCode.Combine(Op, DataType, Immediate),
                _ => HashCode.Combine(DataType, Op),
            };
        }

        public string GetAssembly(OutputValueMode outputMode = OutputValueMode.AsHexAuto)
        {
            string prefix = DataType switch
            {
                DataType.Byte => "byte ",
                DataType.Byte | DataType.Pointer => "byte ptr ",
                DataType.Short => "short ",
                DataType.Short | DataType.Pointer => "short ptr ",
                DataType.Int => "int ",
                DataType.Int | DataType.Pointer => "int ptr ",
                DataType.DoubleWord => "dword ",
                DataType.DoubleWord | DataType.Pointer => "dword ptr ",
                DataType.Far | DataType.Pointer => "far ptr ",
                _ => string.Empty
            };
            string value = Op switch
            {
                OperandType.Register => CPU8086.Register.GetName(Register),
                OperandType.Immediate => Immediate.GetAssembly(outputMode),
                OperandType.Address => Memory.GetAssembly(outputMode),
                _ => string.Empty
            };
            return $"{prefix}{value}";
        }

        public override string ToString()
        {
            string prefix = DataType switch
            {
                DataType.Byte => "byte ",
                DataType.Byte | DataType.Pointer => "byte ptr ",
                DataType.Short => "short ",
                DataType.Short | DataType.Pointer => "short ptr ",
                DataType.Int => "int ",
                DataType.Int | DataType.Pointer => "int ptr ",
                DataType.DoubleWord => "dword ",
                DataType.DoubleWord | DataType.Pointer => "dword ptr ",
                DataType.Far | DataType.Pointer => "far ptr ",
                _ => string.Empty
            };
            string value = Op switch
            {
                OperandType.Register => CPU8086.Register.GetName(Register),
                OperandType.Immediate => Immediate.ToString(),
                OperandType.Address => Memory.ToString(),
                _ => string.Empty
            };
            return $"{prefix}{value}";
        }
    }
}
