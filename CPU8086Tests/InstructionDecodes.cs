using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            Assert.AreEqual<Instruction>(new Instruction(0x00, 2, InstructionType.ADD, DataWidthType.Byte, new InstructionOperand(RegisterType.AL), new InstructionOperand(RegisterType.BL)), cpu.DecodeNext(new byte[] { 0x00, 0b11011000 }, "ADD AL, BL"));
            Assert.AreEqual<Instruction>(new Instruction(0x02, 2, InstructionType.ADD, DataWidthType.Byte, new InstructionOperand(RegisterType.BL), new InstructionOperand(RegisterType.AL)), cpu.DecodeNext(new byte[] { 0x02, 0b11000011 }, "ADD BL, AL"));

            Assert.AreEqual("DAA", cpu.DecodeNext(new byte[] { 0x27 }, "DAA").GetAssembly());
            Assert.AreEqual("PUSH ES", cpu.DecodeNext(new byte[] { 0x06 }, "PUSH es").GetAssembly());
        }

        [TestMethod]
        public void TestLength1Decode()
        {
            CPU cpu = new CPU();

            Assert.AreEqual<Instruction>(
                new Instruction(0x27, 1, InstructionType.DAA, DataWidthType.None), 
                cpu.DecodeNext(new byte[] { 0x27 }, "DAA"));
            Assert.AreEqual<Instruction>(
                new Instruction(0x06, 1, InstructionType.PUSH, DataWidthType.None, new InstructionOperand(RegisterType.ES)), 
                cpu.DecodeNext(new byte[] { 0x06 }, "PUSH es"));

            Assert.AreEqual("DAA", cpu.DecodeNext(new byte[] { 0x27 }, "DAA").GetAssembly());
            Assert.AreEqual("PUSH ES", cpu.DecodeNext(new byte[] { 0x06 }, "PUSH es").GetAssembly());
        }

        [TestMethod]
        public void TestLength2Decode()
        {
            CPU cpu = new CPU();

            Span<byte> mov_CX_BX = new byte[] { 0x89, 0xD9 };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_CX_BX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.CX), new InstructionOperand(RegisterType.BX)),
                cpu.DecodeNext(mov_CX_BX, "MOV CX, BX"));
            Span<byte> mov_CH_AH = new byte[] { 0x88, 0xE5 };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_CH_AH[0], 2, InstructionType.MOV, DataWidthType.Byte, new InstructionOperand(RegisterType.CH), new InstructionOperand(RegisterType.AH)),
                cpu.DecodeNext(mov_CH_AH, "MOV CH, AH"));
            Span<byte> mov_DX_BX = new byte[] { 0x89, 0xDA };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_DX_BX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.DX), new InstructionOperand(RegisterType.BX)),
                cpu.DecodeNext(mov_DX_BX, "MOV DX, BX"));
            Span<byte> mov_SI_BX = new byte[] { 0x89, 0xDE };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_SI_BX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.SI), new InstructionOperand(RegisterType.BX)),
                cpu.DecodeNext(mov_SI_BX, "MOV SI, BX"));
            Span<byte> mov_BX_DI = new byte[] { 0x89, 0xFB };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_BX_DI[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BX), new InstructionOperand(RegisterType.DI)),
                cpu.DecodeNext(mov_BX_DI, "MOV BX, DI"));
            Span<byte> mov_AL_CL = new byte[] { 0x88, 0xC8 };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_AL_CL[0], 2, InstructionType.MOV, DataWidthType.Byte, new InstructionOperand(RegisterType.AL), new InstructionOperand(RegisterType.CL)),
                cpu.DecodeNext(mov_AL_CL, "MOV AL, CL"));
            Span<byte> mov_CH_CH = new byte[] { 0x88, 0xED };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_CH_CH[0], 2, InstructionType.MOV, DataWidthType.Byte, new InstructionOperand(RegisterType.CH), new InstructionOperand(RegisterType.CH)),
                cpu.DecodeNext(mov_CH_CH, "MOV CH, CH"));
            Span<byte> mov_BX_AX = new byte[] { 0x89, 0xC3 };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_BX_AX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BX), new InstructionOperand(RegisterType.AX)),
                cpu.DecodeNext(mov_BX_AX, "MOV BX, AX"));
            Span<byte> mov_BX_SI = new byte[] { 0x89, 0xF3 };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_BX_SI[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BX), new InstructionOperand(RegisterType.SI)),
                cpu.DecodeNext(mov_BX_SI, "MOV BX, SI"));
            Span<byte> mov_SP_DI = new byte[] { 0x89, 0xFC };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_SP_DI[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.SP), new InstructionOperand(RegisterType.DI)),
                cpu.DecodeNext(mov_SP_DI, "MOV SP, DI"));
            Span<byte> mov_BP_AX = new byte[] { 0x89, 0xC5 };
            Assert.AreEqual<Instruction>(
                new Instruction(mov_BP_AX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BP), new InstructionOperand(RegisterType.AX)),
                cpu.DecodeNext(mov_BP_AX, "MOV BP, AX"));

            Span<byte> add_AL_0x2A = new byte[] { 0x04, 0x2A };
            Assert.AreEqual<Instruction>(
                new Instruction(add_AL_0x2A[0], 2, InstructionType.ADD, DataWidthType.Byte, new InstructionOperand(RegisterType.AL), new InstructionOperand(42, ImmediateFlag.None)), 
                cpu.DecodeNext(add_AL_0x2A, "ADD AL, 0x2A"));

            Assert.AreEqual("MOV CX, BX", cpu.DecodeNext(mov_CX_BX, "MOV CX, BX").GetAssembly());
            Assert.AreEqual("MOV CH, AH", cpu.DecodeNext(mov_CH_AH, "MOV CH, AH").GetAssembly());
            Assert.AreEqual("MOV DX, BX", cpu.DecodeNext(mov_DX_BX, "MOV DX, BX").GetAssembly());
            Assert.AreEqual("MOV SI, BX", cpu.DecodeNext(mov_SI_BX, "MOV SI, BX").GetAssembly());
            Assert.AreEqual("MOV BX, DI", cpu.DecodeNext(mov_BX_DI, "MOV BX, DI").GetAssembly());
            Assert.AreEqual("MOV AL, CL", cpu.DecodeNext(mov_AL_CL, "MOV AL, CL").GetAssembly());
            Assert.AreEqual("MOV CH, CH", cpu.DecodeNext(mov_CH_CH, "MOV CH, CH").GetAssembly());
            Assert.AreEqual("MOV BX, SI", cpu.DecodeNext(mov_BX_SI, "MOV BX, SI").GetAssembly());
            Assert.AreEqual("MOV BX, AX", cpu.DecodeNext(mov_BX_AX, "MOV BX, AX").GetAssembly());
            Assert.AreEqual("MOV SP, DI", cpu.DecodeNext(mov_SP_DI, "MOV SP, DI").GetAssembly());
            Assert.AreEqual("MOV BP, AX", cpu.DecodeNext(mov_BP_AX, "MOV BP, AX").GetAssembly());

            Assert.AreEqual("ADD AL, 0x2A", cpu.DecodeNext(add_AL_0x2A, "ADD AL 0x2A").GetAssembly());
        }

        [TestMethod]
        public void TestLength3Decode()
        {
            CPU cpu = new CPU();

            Assert.AreEqual<Instruction>(
                new Instruction(0x05, 3, InstructionType.ADD, DataWidthType.Word, new InstructionOperand(RegisterType.AX), new InstructionOperand((short)-4093, ImmediateFlag.None)),
                cpu.DecodeNext(new byte[] { 0x05, 0x03, 0xF0 }, "ADD"));
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