using Final.CPU8086.Instructions;
using Xunit;

namespace Final.CPU8086
{
    public class CyclesTests
    {
        private static uint DecodeCycles(params byte[] code)
        {
            CPU cpu = new CPU();
            Instruction instruction = cpu.DecodeNext(code, nameof(CyclesTests));
            Assert.NotNull(instruction);
            return instruction.Cycles;
        }

        [Fact]
        public void RegisterToRegisterHasNoEaPenalty()
        {
            // mov ax, bx  -> register to register = 2 clocks
            Assert.Equal(2u, DecodeCycles(0x89, 0xD8));
        }

        [Fact]
        public void WordStoreToEvenAddressHasNoOddPenalty()
        {
            // mov [bx+2], cx  -> base 9 + EA(BX+D8) 9 = 18, even displacement, no penalty
            // (cx, not ax, so the general memory<-register entry is used, not the accumulator one)
            Assert.Equal(18u, DecodeCycles(0x89, 0x4F, 0x02));
        }

        [Fact]
        public void WordStoreToOddAddressAddsFourPerTransfer()
        {
            // mov [bx+1], cx  -> base 9 + EA(BX+D8) 9 = 18, odd displacement, +4 per transfer (1) = 22.
            // Regression for the bug where the odd penalty multiplied the whole cost instead of adding.
            Assert.Equal(22u, DecodeCycles(0x89, 0x4F, 0x01));
        }

        [Fact]
        public void CyclesTableAcceptsValueOperandWithoutThrowing()
        {
            // OperandType has 7 members (None..Value). The table used to be sized for 6,
            // throwing IndexOutOfRange for OperandType.Value. It must now return a default cost.
            CyclesTable table = new CyclesTable();
            CyclesTable.Cycles cycles = table.Get(InstructionType.MOV, OperandType.Value, OperandType.Value);
            Assert.Equal(0, cycles.Value);
        }
    }
}
