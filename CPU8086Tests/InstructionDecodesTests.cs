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
    public class InstructionDecodesTests
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

            {
                Span<byte> mov_DAA = new byte[] { 0x27 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_DAA[0], 1, InstructionType.DAA, DataWidthType.None),
                    actual = cpu.DecodeNext(mov_DAA, nameof(mov_DAA)));
                Assert.AreEqual("DAA", actual.GetAssembly());
            }
            {
                Span<byte> mov_PUSH_ES = new byte[] { 0x06 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_PUSH_ES[0], 1, InstructionType.PUSH, DataWidthType.None, new InstructionOperand(RegisterType.ES)),
                    actual = cpu.DecodeNext(mov_PUSH_ES, nameof(mov_PUSH_ES)));
                Assert.AreEqual("PUSH ES", actual.GetAssembly());
            }

        }

        [TestMethod]
        public void TestLength2Decode()
        {
            CPU cpu = new CPU();

            // MOV
            {
                Span<byte> mov_CX_BX = new byte[] { 0x89, 0xD9 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_CX_BX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.CX), new InstructionOperand(RegisterType.BX)),
                    actual = cpu.DecodeNext(mov_CX_BX, nameof(mov_CX_BX)));
                Assert.AreEqual("MOV CX, BX", actual.GetAssembly());
            }
            {
                Span<byte> mov_CH_AH = new byte[] { 0x88, 0xE5 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_CH_AH[0], 2, InstructionType.MOV, DataWidthType.Byte, new InstructionOperand(RegisterType.CH), new InstructionOperand(RegisterType.AH)),
                    actual = cpu.DecodeNext(mov_CH_AH, nameof(mov_CH_AH)));
                Assert.AreEqual("MOV CH, AH", actual.GetAssembly());
            }
            {
                Span<byte> mov_DX_BX = new byte[] { 0x89, 0xDA };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_DX_BX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.DX), new InstructionOperand(RegisterType.BX)),
                    actual = cpu.DecodeNext(mov_DX_BX, nameof(mov_DX_BX)));
                Assert.AreEqual("MOV DX, BX", actual.GetAssembly());
            }
            {
                Span<byte> mov_SI_BX = new byte[] { 0x89, 0xDE };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_SI_BX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.SI), new InstructionOperand(RegisterType.BX)),
                    actual = cpu.DecodeNext(mov_SI_BX, nameof(mov_SI_BX)));
                Assert.AreEqual("MOV SI, BX", actual.GetAssembly());
            }
            {
                Span<byte> mov_BX_DI = new byte[] { 0x89, 0xFB };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_BX_DI[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BX), new InstructionOperand(RegisterType.DI)),
                    actual = cpu.DecodeNext(mov_BX_DI, nameof(mov_BX_DI)));
                Assert.AreEqual("MOV BX, DI", actual.GetAssembly());
            }
            {
                Span<byte> mov_AL_CL = new byte[] { 0x88, 0xC8 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_AL_CL[0], 2, InstructionType.MOV, DataWidthType.Byte, new InstructionOperand(RegisterType.AL), new InstructionOperand(RegisterType.CL)),
                    actual = cpu.DecodeNext(mov_AL_CL, nameof(mov_AL_CL)));
                Assert.AreEqual("MOV AL, CL", actual.GetAssembly());
            }
            {
                Span<byte> mov_CH_CH = new byte[] { 0x88, 0xED };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_CH_CH[0], 2, InstructionType.MOV, DataWidthType.Byte, new InstructionOperand(RegisterType.CH), new InstructionOperand(RegisterType.CH)),
                    actual = cpu.DecodeNext(mov_CH_CH, nameof(mov_CH_CH)));
                Assert.AreEqual("MOV CH, CH", actual.GetAssembly());
            }
            {
                Span<byte> mov_BX_AX = new byte[] { 0x89, 0xC3 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_BX_AX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BX), new InstructionOperand(RegisterType.AX)),
                    actual = cpu.DecodeNext(mov_BX_AX, nameof(mov_BX_AX)));
                Assert.AreEqual("MOV BX, AX", actual.GetAssembly());
            }
            {
                Span<byte> mov_BX_SI = new byte[] { 0x89, 0xF3 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_BX_SI[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BX), new InstructionOperand(RegisterType.SI)),
                    actual = cpu.DecodeNext(mov_BX_SI, nameof(mov_BX_SI)));
                Assert.AreEqual("MOV BX, SI", actual.GetAssembly());
            }
            {
                Span<byte> mov_SP_DI = new byte[] { 0x89, 0xFC };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_SP_DI[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.SP), new InstructionOperand(RegisterType.DI)),
                    actual = cpu.DecodeNext(mov_SP_DI, nameof(mov_SP_DI)));
                Assert.AreEqual("MOV SP, DI", actual.GetAssembly());
            }
            {
                Span<byte> mov_BP_AX = new byte[] { 0x89, 0xC5 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_BP_AX[0], 2, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.BP), new InstructionOperand(RegisterType.AX)),
                    actual = cpu.DecodeNext(mov_BP_AX, nameof(mov_BP_AX)));
                Assert.AreEqual("MOV BP, AX", actual.GetAssembly());
            }

            // ADD
            {
                Span<byte> add_AL_0x2A = new byte[] { 0x04, 0x2A };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(add_AL_0x2A[0], 2, InstructionType.ADD, DataWidthType.Byte, new InstructionOperand(RegisterType.AL), new InstructionOperand(42, ImmediateFlag.None)),
                    actual = cpu.DecodeNext(add_AL_0x2A, nameof(add_AL_0x2A)));
                Assert.AreEqual("ADD AL, 0x2A", actual.GetAssembly());
            }
        }

        [TestMethod]
        public void TestLength3Decode()
        {
            CPU cpu = new CPU();

            {
                Span<byte> mov_CX_0x0C = new byte[] { 0xB9, 0x0C, 0x00 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_CX_0x0C[0], 3, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.CX), new InstructionOperand((short)12, ImmediateFlag.None)),
                    actual = cpu.DecodeNext(mov_CX_0x0C, nameof(mov_CX_0x0C)));
                Assert.AreEqual("MOV CX, 12", actual.GetAssembly());
                Assert.AreEqual("MOV CX, 12", actual.GetAssembly(OutputValueMode.AsInteger));
                Assert.AreEqual("MOV CX, 0x000C", actual.GetAssembly(OutputValueMode.AsHex));
            }
            {
                Span<byte> mov_CX_0xFFF4 = new byte[] { 0xB9, 0xF4, 0xFF };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_CX_0xFFF4[0], 3, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.CX), new InstructionOperand((short)-12, ImmediateFlag.None)),
                    actual = cpu.DecodeNext(mov_CX_0xFFF4, nameof(mov_CX_0xFFF4)));
                Assert.AreEqual("MOV CX, -12", actual.GetAssembly());
                Assert.AreEqual("MOV CX, -12", actual.GetAssembly(OutputValueMode.AsInteger));
                Assert.AreEqual("MOV CX, 0xFFF4", actual.GetAssembly(OutputValueMode.AsHex));
            }
            {
                Span<byte> mov_DX_0xF6C = new byte[] { 0xBA, 0x6C, 0x0F };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(mov_DX_0xF6C[0], 3, InstructionType.MOV, DataWidthType.Word, new InstructionOperand(RegisterType.DX), new InstructionOperand((short)3948, ImmediateFlag.None)),
                    actual = cpu.DecodeNext(mov_DX_0xF6C, "MOV DX, 0xF6C"));
                Assert.AreEqual("MOV DX, 3948", actual.GetAssembly());
            }
            {
                Span<byte> add_AX_neg4093 = new byte[] { 0x05, 0x03, 0xF0 };
                Instruction actual;
                Assert.AreEqual<Instruction>(
                    new Instruction(add_AX_neg4093[0], 3, InstructionType.ADD, DataWidthType.Word, new InstructionOperand(RegisterType.AX), new InstructionOperand((short)-4093, ImmediateFlag.None)),
                    actual = cpu.DecodeNext(add_AX_neg4093, nameof(add_AX_neg4093)));
                Assert.AreEqual("ADD AX, -4093", actual.GetAssembly());
            }
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