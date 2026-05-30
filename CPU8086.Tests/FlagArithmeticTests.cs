using Final.CPU8086.Execution;
using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using System;
using Xunit;

namespace Final.CPU8086
{
    public class FlagArithmeticTests
    {
        private static CPU ExecuteSingle(Action<RegisterState> setup, params byte[] code)
        {
            CPU cpu = new CPU();
            setup(cpu.Register);
            InstructionExecuter executer = new InstructionExecuter(cpu);
            Instruction instruction = cpu.DecodeNext(code, nameof(FlagArithmeticTests));
            Assert.NotNull(instruction);
            RunState state = new RunState();
            OneOf.OneOf<int, Error> result = executer.Execute(instruction, state);
            Assert.True(result.IsT0, result.IsT1 ? result.AsT1.ToString() : "execution failed");
            return cpu;
        }

        [Fact]
        public void AddByteSetsCarryOnUnsignedOverflow()
        {
            // add al, cl ; AL=0x80, CL=0x80 -> 0x100. Old signed code computed -256 and missed carry.
            CPU cpu = ExecuteSingle(r => { r.AL = unchecked((sbyte)0x80); r.CL = unchecked((sbyte)0x80); }, 0x00, 0xC8);
            Assert.Equal(0, cpu.Register.AL);
            Assert.True(cpu.Register.CarryFlag);
            Assert.True(cpu.Register.ZeroFlag);
            Assert.True(cpu.Register.OverflowFlag);   // -128 + -128 signed overflow
            Assert.False(cpu.Register.SignFlag);
            Assert.False(cpu.Register.AuxiliaryCarryFlag);
        }

        [Fact]
        public void AddByteSetsOverflowAndSignOnSignedOverflow()
        {
            // add al, cl ; AL=0x7F, CL=0x01 -> 0x80. Signed overflow, no unsigned carry.
            CPU cpu = ExecuteSingle(r => { r.AL = 0x7F; r.CL = 0x01; }, 0x00, 0xC8);
            Assert.Equal(unchecked((sbyte)0x80), cpu.Register.AL);
            Assert.False(cpu.Register.CarryFlag);
            Assert.True(cpu.Register.OverflowFlag);
            Assert.True(cpu.Register.SignFlag);
            Assert.True(cpu.Register.AuxiliaryCarryFlag);  // 0xF + 0x1 crosses nibble
            Assert.False(cpu.Register.ZeroFlag);
        }

        [Fact]
        public void CmpByteUsesUnsignedBorrowForCarry()
        {
            // cmp al, cl ; AL=0xFF (-1), CL=0x01. Unsigned 0xFF - 0x01 does NOT borrow.
            // Old code did a signed compare (-1 < 1) and wrongly set carry.
            CPU cpu = ExecuteSingle(r => { r.AL = unchecked((sbyte)0xFF); r.CL = 0x01; }, 0x38, 0xC8);
            Assert.Equal(unchecked((sbyte)0xFF), cpu.Register.AL);  // cmp does not modify the destination
            Assert.False(cpu.Register.CarryFlag);
            Assert.True(cpu.Register.SignFlag);
            Assert.False(cpu.Register.ZeroFlag);
            Assert.False(cpu.Register.OverflowFlag);
        }

        [Fact]
        public void SubByteBorrowSetsCarryAndAuxiliary()
        {
            // sub al, cl ; AL=0x01, CL=0x02 -> 0xFF with borrow.
            CPU cpu = ExecuteSingle(r => { r.AL = 0x01; r.CL = 0x02; }, 0x28, 0xC8);
            Assert.Equal(unchecked((sbyte)0xFF), cpu.Register.AL);
            Assert.True(cpu.Register.CarryFlag);
            Assert.True(cpu.Register.AuxiliaryCarryFlag);
            Assert.True(cpu.Register.SignFlag);
            Assert.False(cpu.Register.ZeroFlag);
            Assert.False(cpu.Register.OverflowFlag);
        }
    }
}
