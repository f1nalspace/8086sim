using Final.CPU8086.Execution;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final.CPU8086
{
    public class FlagsTests
    {
        [Fact]
        public void IsParity()
        {
            Assert.False(InstructionExecuter.IsParity8(0b00011010));
            Assert.False(InstructionExecuter.IsParity8((byte)26));
            Assert.False(InstructionExecuter.IsParity16((ushort)26));

            Assert.True(InstructionExecuter.IsParity8(0b00001010));
            Assert.True(InstructionExecuter.IsParity8((byte)10));
            Assert.True(InstructionExecuter.IsParity16((ushort)10));
        }
    }
}
