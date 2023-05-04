using System;

namespace Final.CPU8086.Instructions
{
    public class CyclesTable
    {
        public readonly struct Cycles
        {
            public ushort Value { get; }
            public byte Transfers { get; }
            public byte EA { get; }

            public Cycles(ushort value, byte transfers = 0, bool ea = false)
            {
                Value = value;
                Transfers = transfers;
                EA = ea ? (byte)1 : (byte)0;
            }
        }

        public Cycles[,,] _cyclesFromOperand = new Cycles[0xFF, 6, 6];

        private void Set(InstructionType type, OperandType a, OperandType b, Cycles cycles)
        {
            _cyclesFromOperand[(int)type, (int)a, (int)b] = cycles;
        }

        public Cycles Get(InstructionType type, OperandType a, OperandType b) => _cyclesFromOperand[(int)type, (int)a, (int)b];

        public CyclesTable()
        {
            Array.Clear(_cyclesFromOperand);

            Set(InstructionType.AAA, OperandType.None, OperandType.None, new Cycles(4));
            Set(InstructionType.AAD, OperandType.None, OperandType.None, new Cycles(60));
            Set(InstructionType.AAM, OperandType.None, OperandType.None, new Cycles(83));
            Set(InstructionType.AAS, OperandType.None, OperandType.None, new Cycles(4));

            Set(InstructionType.ADC, OperandType.Register, OperandType.Register, new Cycles(3));
            Set(InstructionType.ADC, OperandType.Register, OperandType.Memory, new Cycles(9, 1, true));
            Set(InstructionType.ADC, OperandType.Memory, OperandType.Register, new Cycles(16, 2, true));
            Set(InstructionType.ADC, OperandType.Register, OperandType.Immediate, new Cycles(4));
            Set(InstructionType.ADC, OperandType.Memory, OperandType.Immediate, new Cycles(17, 2, true));
            Set(InstructionType.ADC, OperandType.Accumulator, OperandType.Immediate, new Cycles(4));

            Set(InstructionType.ADD, OperandType.Register, OperandType.Register, new Cycles(3));
            Set(InstructionType.ADD, OperandType.Register, OperandType.Memory, new Cycles(9, 1, true));
            Set(InstructionType.ADD, OperandType.Memory, OperandType.Register, new Cycles(16, 2, true));
            Set(InstructionType.ADD, OperandType.Register, OperandType.Immediate, new Cycles(4));
            Set(InstructionType.ADD, OperandType.Memory, OperandType.Immediate, new Cycles(17, 2, true));
            Set(InstructionType.ADD, OperandType.Accumulator, OperandType.Immediate, new Cycles(4));

            Set(InstructionType.AND, OperandType.Register, OperandType.Register, new Cycles(3));
            Set(InstructionType.AND, OperandType.Register, OperandType.Memory, new Cycles(9, 1, true));
            Set(InstructionType.AND, OperandType.Memory, OperandType.Register, new Cycles(16, 2, true));
            Set(InstructionType.AND, OperandType.Register, OperandType.Immediate, new Cycles(4));
            Set(InstructionType.AND, OperandType.Memory, OperandType.Immediate, new Cycles(17, 2, true));
            Set(InstructionType.AND, OperandType.Accumulator, OperandType.Immediate, new Cycles(4));

            Set(InstructionType.MOV, OperandType.Memory, OperandType.Accumulator, new Cycles(10, 1));
            Set(InstructionType.MOV, OperandType.Accumulator, OperandType.Memory, new Cycles(10, 1));
            Set(InstructionType.MOV, OperandType.Register, OperandType.Register, new Cycles(2));
            Set(InstructionType.MOV, OperandType.Register, OperandType.Accumulator, new Cycles(2));
            Set(InstructionType.MOV, OperandType.Accumulator, OperandType.Register, new Cycles(2));
            Set(InstructionType.MOV, OperandType.Register, OperandType.Memory, new Cycles(8, 1, true));
            Set(InstructionType.MOV, OperandType.Memory, OperandType.Register, new Cycles(9, 1, true));
            Set(InstructionType.MOV, OperandType.Register, OperandType.Immediate, new Cycles(4));
            Set(InstructionType.MOV, OperandType.Accumulator, OperandType.Immediate, new Cycles(4));
            Set(InstructionType.MOV, OperandType.Memory, OperandType.Immediate, new Cycles(10, 1, true));
            Set(InstructionType.MOV, OperandType.Segment, OperandType.Register, new Cycles(2));
            Set(InstructionType.MOV, OperandType.Segment, OperandType.Memory, new Cycles(8, 1, true));
            Set(InstructionType.MOV, OperandType.Register, OperandType.Segment, new Cycles(2));
            Set(InstructionType.MOV, OperandType.Memory, OperandType.Segment, new Cycles(9, 1, true));
        }
    }
}
