using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPU8086
{
    [TestClass]
    public class InstructionDecodes
    {
        private static readonly InstructionTable instructionTable = new InstructionTable();

        [TestMethod]
        public void OpCodeTranslations()
        {
            Assert.AreEqual(OpCode.MOV_dREG16_dMEM16_sREG16, instructionTable[0x89].OpCode);
        }
    }
}