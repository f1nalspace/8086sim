using Final.CPU8086.Execution;
using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using Xunit;

namespace Final.CPU8086
{
    public class CallRetTests
    {
        private static uint StackPhysical(CPU cpu)
            => cpu.GetAbsoluteMemoryAddress(new MemoryAddress(
                EffectiveAddressCalculation.DirectAddress, new Immediate(cpu.Register.SP), SegmentType.SS, 0));

        [Fact]
        public void CallPushesReturnAddressAndReturnsRelativeDisplacement()
        {
            CPU cpu = new CPU();
            InstructionExecuter executer = new InstructionExecuter(cpu);

            // Simulate Step(): IP already advanced past the 3-byte CALL to its return address.
            cpu.Register.IP = 0x0003;
            short spBefore = cpu.Register.SP;

            // call rel16  (E8 disp16), disp = +5
            Instruction instruction = cpu.DecodeNext(new byte[] { 0xE8, 0x05, 0x00 }, nameof(CallRetTests));
            Assert.NotNull(instruction);

            OneOf.OneOf<int, Error> res = executer.Execute(instruction, new RunState());
            Assert.True(res.IsT0, res.IsT1 ? res.AsT1.ToString() : "execution failed");

            Assert.Equal(spBefore - 2, cpu.Register.SP);  // one word pushed

            // The pushed word must be the return address (0x0003). The old bug pushed SP instead.
            uint phys = StackPhysical(cpu);
            int pushed = cpu.Memory[phys] | (cpu.Memory[phys + 1] << 8);
            Assert.Equal(0x0003, pushed);
            Assert.NotEqual(spBefore, (short)pushed);     // guards against the regressed "push SP" behavior
        }

        [Fact]
        public void RetPopsAbsoluteIpAndReturnsRelativeDelta()
        {
            CPU cpu = new CPU();
            InstructionExecuter executer = new InstructionExecuter(cpu);

            cpu.Register.SP = 0x0100;
            uint phys = StackPhysical(cpu);
            cpu.Memory[phys] = 0x30;
            cpu.Memory[phys + 1] = 0x00;        // return target = 0x0030
            cpu.Register.IP = 0x0010;           // simulate post-RET IP

            Instruction instruction = cpu.DecodeNext(new byte[] { 0xC3 }, nameof(CallRetTests)); // ret (near)
            Assert.NotNull(instruction);

            OneOf.OneOf<int, Error> res = executer.Execute(instruction, new RunState());
            Assert.True(res.IsT0, res.IsT1 ? res.AsT1.ToString() : "execution failed");

            Assert.Equal(0x30 - 0x10, res.AsT0);    // delta so Step() lands on absolute 0x0030
            Assert.Equal(0x0102, cpu.Register.SP);  // SP incremented by one word
        }

        [Fact]
        public void RetfReturnsErrorInsteadOfThrowing()
        {
            CPU cpu = new CPU();
            InstructionExecuter executer = new InstructionExecuter(cpu);

            Instruction instruction = cpu.DecodeNext(new byte[] { 0xCB }, nameof(CallRetTests)); // retf
            Assert.NotNull(instruction);

            // Must surface an Error, not throw NotImplementedException.
            OneOf.OneOf<int, Error> res = executer.Execute(instruction, new RunState());
            Assert.True(res.IsT1);
        }
    }
}
