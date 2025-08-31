using System.Collections.Generic;
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

        public Stream Get(string name, bool includeNamespace = true)
        { 
            string resourceName = includeNamespace ? Namespace + name : name;
            return _asm.GetManifestResourceStream(resourceName);
        }

        public IEnumerable<InstructionStreamResourceName> GetNames()
        {
            string[] fullNames = _asm.GetManifestResourceNames();

            var strippedNames = fullNames.Select(s => new KeyValuePair<string, string>(s.Remove(0, Namespace.Length), s));

            var groups = strippedNames.GroupBy(f => f.Key.Substring(0, f.Key.IndexOf('.')));

            List<InstructionStreamResourceName> result = new List<InstructionStreamResourceName>();

            foreach (var group in groups)
            {
                foreach (var pair in group)
                {
                    string name = pair.Key.Remove(0, group.Key.Length + 1);
                    string resourceName = pair.Value;
                    result.Add(new InstructionStreamResourceName(resourceName, group.Key, name));
                }
            }

            return result;
        }
    }
}
