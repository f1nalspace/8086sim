using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IS = Final.CPU8086.Instruction;
using IT = Final.CPU8086.InstructionType;
using IO = Final.CPU8086.InstructionOperand;
using RT = Final.CPU8086.RegisterType;
using DWT = Final.CPU8086.DataWidthType;

namespace Final.CPU8086
{
    [TestClass]
    public class InstructionDecodesTests
    {
        private static readonly InstructionStreamResources _resources = new InstructionStreamResources();

        private readonly CPU _cpu;

        public InstructionDecodesTests()
        {
            _cpu = new CPU();
        }

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

        private void TestAssembly(string name)
        {
            using Stream machineCodeStream = _resources.Get(name);
            using Stream assemblyCodeStream = _resources.Get(name + ".asm");
            using StreamReader reader = new StreamReader(assemblyCodeStream, leaveOpen: true);
            string expectedAssembly = reader.ReadToEnd();
            OneOf.OneOf<string, Error> res = _cpu.GetAssembly(machineCodeStream, name, OutputValueMode.AsInteger);
            res.Switch(
                actualAssembly => AssertAssembly(expectedAssembly, actualAssembly),
                error => Assert.Fail(error.ToString()));
        }

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void WithLength2To4()
        {
            {
                IS actual;
                Span<byte> add_AL_BL = stackalloc byte[] { 0x00, 0b11011000 };
                Assert.AreEqual(
                    new IS(add_AL_BL[0], 2, IT.ADD, DWT.Byte, new IO(RT.AL), new IO(RT.BL)),
                    actual = _cpu.DecodeNext(add_AL_BL, nameof(add_AL_BL)));
                Assert.AreEqual("ADD AL, BL", actual.Asm());
            }

            {
                IS actual;
                Span<byte> add_BL_AL = stackalloc byte[] { 0x02, 0b11000011 };
                Assert.AreEqual(
                    new IS(add_BL_AL[0], 2, IT.ADD, DWT.Byte, new IO(RT.BL), new IO(RT.AL)),
                    actual = _cpu.DecodeNext(add_BL_AL, nameof(add_BL_AL)));
                Assert.AreEqual("ADD BL, AL", actual.Asm());
            }
        }

        [TestMethod]
        public void WithLength1()
        {
            {
                Span<byte> mov_DAA = stackalloc byte[] { 0x27 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_DAA[0], 1, IT.DAA, DWT.None),
                    actual = _cpu.DecodeNext(mov_DAA, nameof(mov_DAA)));
                Assert.AreEqual("DAA", actual.Asm());
            }

            {
                Span<byte> push_ES = stackalloc byte[] { 0x06 };
                IS actual;
                Assert.AreEqual(
                    new IS(push_ES[0], 1, IT.PUSH, DWT.None, new IO(RT.ES)),
                    actual = _cpu.DecodeNext(push_ES, nameof(push_ES)));
                Assert.AreEqual("PUSH ES", actual.Asm());
            }

        }

        [TestMethod]
        public void WithLength2()
        {
            CPU cpu = new CPU();

            // MOV, 2 bytes
            {
                Span<byte> mov_CX_BX = stackalloc byte[] { 0x89, 0xD9 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_CX_BX[0], 2, IT.MOV, DWT.Word, new IO(RT.CX), new IO(RT.BX)),
                    actual = _cpu.DecodeNext(mov_CX_BX, nameof(mov_CX_BX)));
                Assert.AreEqual("MOV CX, BX", actual.Asm());
            }
            {
                Span<byte> mov_CH_AH = stackalloc byte[] { 0x88, 0xE5 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_CH_AH[0], 2, IT.MOV, DWT.Byte, new IO(RT.CH), new IO(RT.AH)),
                    actual = _cpu.DecodeNext(mov_CH_AH, nameof(mov_CH_AH)));
                Assert.AreEqual("MOV CH, AH", actual.Asm());
            }
            {
                Span<byte> mov_DX_BX = stackalloc byte[] { 0x89, 0xDA };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_DX_BX[0], 2, IT.MOV, DWT.Word, new IO(RT.DX), new IO(RT.BX)),
                    actual = _cpu.DecodeNext(mov_DX_BX, nameof(mov_DX_BX)));
                Assert.AreEqual("MOV DX, BX", actual.Asm());
            }
            {
                Span<byte> mov_SI_BX = stackalloc byte[] { 0x89, 0xDE };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_SI_BX[0], 2, IT.MOV, DWT.Word, new IO(RT.SI), new IO(RT.BX)),
                    actual = _cpu.DecodeNext(mov_SI_BX, nameof(mov_SI_BX)));
                Assert.AreEqual("MOV SI, BX", actual.Asm());
            }
            {
                Span<byte> mov_BX_DI = stackalloc byte[] { 0x89, 0xFB };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_BX_DI[0], 2, IT.MOV, DWT.Word, new IO(RT.BX), new IO(RT.DI)),
                    actual = _cpu.DecodeNext(mov_BX_DI, nameof(mov_BX_DI)));
                Assert.AreEqual("MOV BX, DI", actual.Asm());
            }
            {
                Span<byte> mov_AL_CL = stackalloc byte[] { 0x88, 0xC8 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_AL_CL[0], 2, IT.MOV, DWT.Byte, new IO(RT.AL), new IO(RT.CL)),
                    actual = _cpu.DecodeNext(mov_AL_CL, nameof(mov_AL_CL)));
                Assert.AreEqual("MOV AL, CL", actual.Asm());
            }
            {
                Span<byte> mov_CH_CH = stackalloc byte[] { 0x88, 0xED };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_CH_CH[0], 2, IT.MOV, DWT.Byte, new IO(RT.CH), new IO(RT.CH)),
                    actual = _cpu.DecodeNext(mov_CH_CH, nameof(mov_CH_CH)));
                Assert.AreEqual("MOV CH, CH", actual.Asm());
            }
            {
                Span<byte> mov_BX_AX = stackalloc byte[] { 0x89, 0xC3 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_BX_AX[0], 2, IT.MOV, DWT.Word, new IO(RT.BX), new IO(RT.AX)),
                    actual = _cpu.DecodeNext(mov_BX_AX, nameof(mov_BX_AX)));
                Assert.AreEqual("MOV BX, AX", actual.Asm());
            }
            {
                Span<byte> mov_BX_SI = stackalloc byte[] { 0x89, 0xF3 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_BX_SI[0], 2, IT.MOV, DWT.Word, new IO(RT.BX), new IO(RT.SI)),
                    actual = _cpu.DecodeNext(mov_BX_SI, nameof(mov_BX_SI)));
                Assert.AreEqual("MOV BX, SI", actual.Asm());
            }
            {
                Span<byte> mov_SP_DI = stackalloc byte[] { 0x89, 0xFC };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_SP_DI[0], 2, IT.MOV, DWT.Word, new IO(RT.SP), new IO(RT.DI)),
                    actual = _cpu.DecodeNext(mov_SP_DI, nameof(mov_SP_DI)));
                Assert.AreEqual("MOV SP, DI", actual.Asm());
            }
            {
                Span<byte> mov_BP_AX = stackalloc byte[] { 0x89, 0xC5 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_BP_AX[0], 2, IT.MOV, DWT.Word, new IO(RT.BP), new IO(RT.AX)),
                    actual = _cpu.DecodeNext(mov_BP_AX, nameof(mov_BP_AX)));
                Assert.AreEqual("MOV BP, AX", actual.Asm());
            }

            // ADD, 2 bytes
            {
                Span<byte> add_AL_0x2A = stackalloc byte[] { 0x04, 0x2A };
                IS actual;
                Assert.AreEqual(
                    new IS(add_AL_0x2A[0], 2, IT.ADD, DWT.Byte, new IO(RT.AL), new IO((byte)42, ImmediateFlag.None)),
                    actual = _cpu.DecodeNext(add_AL_0x2A, nameof(add_AL_0x2A)));
                Assert.AreEqual("ADD AL, 42", actual.Asm());
                Assert.AreEqual("ADD AL, 42", actual.Asm(OutputValueMode.AsInteger));
                Assert.AreEqual("ADD AL, 0x2A", actual.Asm(OutputValueMode.AsHex));
            }

            // JNL, 2 bytes
            {
                Span<byte> add_JNL_FFEE = stackalloc byte[] { 0x7D, 0xEC };
                IS actual;
                Assert.AreEqual(
                    new IS(add_JNL_FFEE[0], 2, IT.JGE, DWT.None, new IO(unchecked((sbyte)0xEC), ImmediateFlag.RelativeJumpDisplacement)),
                    actual = _cpu.DecodeNext(add_JNL_FFEE, nameof(add_JNL_FFEE)));
                Assert.AreEqual("JGE -20", actual.Asm());
                Assert.AreEqual("JGE -20", actual.Asm(OutputValueMode.AsInteger));
            }
        }

        [TestMethod]
        public void WithLength3()
        {
            

            // MOV, 3 bytes
            {
                Span<byte> mov_CX_0x0C = stackalloc byte[] { 0xB9, 0x0C, 0x00 };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_CX_0x0C[0], 3, IT.MOV, DWT.Word, new IO(RT.CX), new IO((ushort)12, ImmediateFlag.None)),
                    actual = _cpu.DecodeNext(mov_CX_0x0C, nameof(mov_CX_0x0C)));
                Assert.AreEqual("MOV CX, 12", actual.Asm());
                Assert.AreEqual("MOV CX, 12", actual.Asm(OutputValueMode.AsInteger));
                Assert.AreEqual("MOV CX, 0x000C", actual.Asm(OutputValueMode.AsHex));
            }
            {
                Span<byte> mov_CX_0xFFF4 = stackalloc byte[] { 0xB9, 0xF4, 0xFF };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_CX_0xFFF4[0], 3, IT.MOV, DWT.Word, new IO(RT.CX), new IO((short)-12, ImmediateFlag.None)),
                    actual = _cpu.DecodeNext(mov_CX_0xFFF4, nameof(mov_CX_0xFFF4)));
                Assert.AreEqual("MOV CX, -12", actual.Asm());
                Assert.AreEqual("MOV CX, -12", actual.Asm(OutputValueMode.AsInteger));
                Assert.AreEqual("MOV CX, 0xFFF4", actual.Asm(OutputValueMode.AsHex));
            }
            {
                Span<byte> mov_DX_0xF6C = stackalloc byte[] { 0xBA, 0x6C, 0x0F };
                IS actual;
                Assert.AreEqual(
                    new IS(mov_DX_0xF6C[0], 3, IT.MOV, DWT.Word, new IO(RT.DX), new IO((ushort)3948, ImmediateFlag.None)),
                    actual = _cpu.DecodeNext(mov_DX_0xF6C, nameof(mov_DX_0xF6C)));
                Assert.AreEqual("MOV DX, 3948", actual.Asm());
                Assert.AreEqual("MOV DX, 3948", actual.Asm(OutputValueMode.AsInteger));
                Assert.AreEqual("MOV DX, 0x0F6C", actual.Asm(OutputValueMode.AsHex));
            }

            // ADD, 3 bytes
            {
                Span<byte> add_AX_neg4093 = stackalloc byte[] { 0x05, 0x03, 0xF0 };
                IS actual;
                Assert.AreEqual(
                    new IS(add_AX_neg4093[0], 3, IT.ADD, DWT.Word, new IO(RT.AX), new IO((short)-4093, ImmediateFlag.None)),
                    actual = _cpu.DecodeNext(add_AX_neg4093, nameof(add_AX_neg4093)));
                Assert.AreEqual("ADD AX, -4093", actual.Asm());
                Assert.AreEqual("ADD AX, -4093", actual.Asm(OutputValueMode.AsInteger));
                Assert.AreEqual("ADD AX, 0xF003", actual.Asm(OutputValueMode.AsHex));
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