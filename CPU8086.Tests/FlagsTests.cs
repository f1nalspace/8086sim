using Final.CPU8086.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final.CPU8086
{
    [TestClass]
    public class FlagsTests
    {
        [TestMethod]
        public void IsParity()
        {
            Assert.AreEqual(false, InstructionExecuter.IsParity8(0b00011010));
            Assert.AreEqual(false, InstructionExecuter.IsParity8((byte)26));
            Assert.AreEqual(false, InstructionExecuter.IsParity16((ushort)26));

            Assert.AreEqual(true, InstructionExecuter.IsParity8(0b00001010));
            Assert.AreEqual(true, InstructionExecuter.IsParity8((byte)10));
            Assert.AreEqual(true, InstructionExecuter.IsParity16((ushort)10));
        }
    }
}
