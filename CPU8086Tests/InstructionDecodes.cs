using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CPU8086
{
    [TestClass]
    public class InstructionDecodes
    {
        private static readonly CPU cpu = new CPU();
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
            OneOf.OneOf<string, Error> res = cpu.GetAssembly(machineCodeStream, name, OutputValueMode.AsInteger);
            res.Switch(
                actualAssembly => AssertAssembly(expectedAssembly, actualAssembly),
                error => Assert.Fail(error.ToString()));
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