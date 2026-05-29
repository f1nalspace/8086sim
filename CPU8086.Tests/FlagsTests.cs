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
            Assert.Equal(false, InstructionExecuter.IsParity8(0b00011010));
            Assert.Equal(false, InstructionExecuter.IsParity8((byte)26));
            Assert.Equal(false, InstructionExecuter.IsParity16((ushort)26));

            Assert.Equal(true, InstructionExecuter.IsParity8(0b00001010));
            Assert.Equal(true, InstructionExecuter.IsParity8((byte)10));
            Assert.Equal(true, InstructionExecuter.IsParity16((ushort)10));
        }
    }
}
