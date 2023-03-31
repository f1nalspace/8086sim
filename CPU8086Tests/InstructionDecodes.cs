using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Final.CPU8086
{
    [TestClass]
    public class InstructionDecodes
    {
        private static readonly InstructionStreamResources _resources = new InstructionStreamResources();

        private static IEnumerable<string> CleanAssembly(string value)
        {
            string[] lines = value.Split('\n');
            foreach (string line in lines)
            {
                if (line.Trim().Length == 0) continue;
                if (line.Trim().StartsWith(';')) continue;
                yield return line.Trim();
            }
        }

        private static void AssertAssembly(string expected, string actual)
        {
            if (expected == null)
                throw new ArgumentNullException(nameof(expected));
            if (actual == null)
                throw new ArgumentNullException(nameof(actual));
            string[] expectedLines = CleanAssembly(expected).ToArray();
            string[] actualLines = CleanAssembly(actual).ToArray();
            if (expectedLines.Length != actualLines.Length)
                Assert.Fail($"Expect {expectedLines.Length} lines, but got {actualLines.Length}");
            for (int i = 0; i < expectedLines.Length; i++)
            {
                string expectedLine = expectedLines[i];
                string actualLine = actualLines[i];
                if (!string.Equals(expectedLine, actualLine, StringComparison.InvariantCultureIgnoreCase))
                    Assert.Fail($"Expect line '{expectedLine}', but got '{actualLine}'");
            }
        }

        private static void TestAssembly(string name)
        {
            using Stream machineCodeStream = _resources.Get(name);
            using Stream assemblyCodeStream = _resources.Get(name + ".asm");
            using StreamReader reader = new StreamReader(assemblyCodeStream, leaveOpen: true);
            string expectedAssembly = reader.ReadToEnd();
            OneOf.OneOf<string, Error> res = CPU.GetAssembly(machineCodeStream, name, OutputValueMode.AsInteger);
            res.Switch(
                actualAssembly => AssertAssembly(expectedAssembly, actualAssembly),
                error => Assert.Fail(error.ToString()));
        }

        [TestMethod]
        public void TestLength2To4Decode()
        {
            CPU cpu = new CPU();
            Assert.AreEqual<Instruction>(cpu.DecodeNext(new byte[] { 0x00, 0b11011000 }, "ADD AL, BL"), new Instruction(0x00, 2, InstructionType.ADD, DataWidthType.Byte, new InstructionOperand(RegisterType.AL), new InstructionOperand(RegisterType.BL)));
            Assert.AreEqual<Instruction>(cpu.DecodeNext(new byte[] { 0x02, 0b11000011 }, "ADD BL, AL"), new Instruction(0x02, 2, InstructionType.ADD, DataWidthType.Byte, new InstructionOperand(RegisterType.BL), new InstructionOperand(RegisterType.AL)));
        }

        [TestMethod]
        public void TestLength1Decode()
        {
            CPU cpu = new CPU();
            Assert.AreEqual<Instruction>(cpu.DecodeNext(new byte[] { 0x27 }, "DAA"), new Instruction(0x27, 1, InstructionType.DAA, DataWidthType.None));
            Assert.AreEqual<Instruction>(cpu.DecodeNext(new byte[] { 0x06 }, "PUSH es"), new Instruction(0x06, 1, InstructionType.PUSH, DataWidthType.None, new InstructionOperand(RegisterType.ES)));
        }

        [TestMethod]
        public void TestLength2Decode()
        {
            CPU cpu = new CPU();
            Assert.AreEqual<Instruction>(cpu.DecodeNext(new byte[] { 0x04, 42 }, "ADD"), new Instruction(0x04, 2, InstructionType.ADD, DataWidthType.Byte, new InstructionOperand(RegisterType.AL), new InstructionOperand(42, ImmediateFlag.None)));
        }

        [TestMethod]
        public void TestLength3Decode()
        {
            CPU cpu = new CPU();
            Assert.AreEqual<Instruction>(cpu.DecodeNext(new byte[] { 0x05, 0x03, 0xF0 }, "ADD"), new Instruction(0x05, 3, InstructionType.ADD, DataWidthType.Word, new InstructionOperand(RegisterType.AX), new InstructionOperand((short)-4093, ImmediateFlag.None)));
        }

        [TestMethod]
        public void Test_listing_0037_single_register_mov()
            => TestAssembly("listing_0037_single_register_mov");

        [TestMethod]
        public void Test_listing_0038_many_register_mov()
            => TestAssembly("listing_0038_many_register_mov");

        [TestMethod]
        public void Test_listing_0039_more_movs()
            => TestAssembly("listing_0039_more_movs");

        [TestMethod]
        public void Test_listing_0040_challenge_movs()
            => TestAssembly("listing_0040_challenge_movs");

        [TestMethod]
        public void Test_listing_0041_add_sub_cmp_jnz()
            => TestAssembly("listing_0041_add_sub_cmp_jnz");

        [TestMethod]
        public void Test_listing_0042_completionist_decode()
            => TestAssembly("listing_0042_completionist_decode");
    }
}