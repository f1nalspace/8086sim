using System;
using System.Runtime.InteropServices;
using Final.CPU8086.Types;

namespace Final.CPU8086.Instructions
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct InstructionOperand : IEquatable<InstructionOperand>
    {
        [FieldOffset(0)]
        public readonly OperandType Op;
        [FieldOffset(1)]
        public readonly MemoryAddress Memory;
        [FieldOffset(1)]
        public readonly RegisterType Register;
        [FieldOffset(1)]
        public readonly Immediate Immediate;
        [FieldOffset(1)]
        public readonly int Value;

        public InstructionOperand(RegisterType register)
        {
            Op = OperandType.Register;
            Memory = new MemoryAddress();
            Immediate = new Immediate();
            Value = 0;
            Register = register;
        }

        public InstructionOperand(Register register)
            : this(register?.Type ?? RegisterType.Unknown) { }

        public InstructionOperand(Immediate immediate)
        {
            Op = OperandType.Immediate;
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

        public InstructionOperand(MemoryAddress address)
        {
            Op = OperandType.Address;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Value = 0;
            Memory = address;
        }

        public InstructionOperand(int value)
        {
            Op = OperandType.Address;
            Register = RegisterType.Unknown;
            Immediate = new Immediate();
            Memory = new MemoryAddress();
            Value = value;
        }

        public InstructionOperand(EffectiveAddressCalculation eac, int displacement)
            : this(new MemoryAddress(eac, displacement)) { }

        public bool Equals(InstructionOperand other)
        {
            if (Op != other.Op) return false;
            switch (Op)
            {
                case OperandType.Register:
                    if (!Register.Equals(other.Register))
                        return false;
                    break;
                case OperandType.Address:
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
            return Op switch
            {
                OperandType.Register => HashCode.Combine(Op, Register),
                OperandType.Address => HashCode.Combine(Op, Memory),
                OperandType.Immediate => HashCode.Combine(Op, Immediate),
                OperandType.Value => HashCode.Combine(Op, Value),
                _ => HashCode.Combine(Op),
            };
        }



        public override string ToString()
        {
            return Op switch
            {
                OperandType.Register => $"Reg: {Types.Register.GetName(Register)}",
                OperandType.Immediate => $"Imm: {Immediate}",
                OperandType.Address => $"Mem: {Memory}",
                OperandType.Value => $"Value: {Value}",
                _ => string.Empty
            };
        }
    }
}
