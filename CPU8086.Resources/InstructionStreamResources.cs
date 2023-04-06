using System.IO;
using System.Linq;
using System.Reflection;

namespace Final.CPU8086
{
    public class InstructionStreamResources
    {
        private readonly Assembly _asm;
        private const string Namespace = "Final.CPU8086.Resources.";

        public InstructionStreamResources()
        {
            _asm = typeof(InstructionStreamResources).Assembly;
        }

        public Stream Get(string name)
            => _asm.GetManifestResourceStream(Namespace + name);

        public string[] GetNames()
        {
            string[] names = _asm.GetManifestResourceNames();
            return names.Select(s => s.Remove(0, Namespace.Length)).ToArray();
        }
    }
}
