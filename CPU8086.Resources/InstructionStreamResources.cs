using System.IO;
using System.Reflection;

namespace Final.CPU8086
{
    public class InstructionStreamResources
    {
        private readonly Assembly _asm;

        public InstructionStreamResources()
        {
            _asm = typeof(InstructionStreamResources).Assembly;
        }

        public Stream Get(string name)
        {
            return _asm.GetManifestResourceStream("Final.CPU8086.Resources." + name);
        }
    }
}
