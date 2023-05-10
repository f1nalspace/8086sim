using System.Collections;
using System.Collections.Generic;

namespace Final.CPU8086.Instructions
{
    public class InstructionDefinitionList : IReadOnlyCollection<InstructionDefinition>
    {
        private readonly List<InstructionDefinition> _instructions = new List<InstructionDefinition>();

        public byte Op { get; }

        public int Count => _instructions.Count;

        public InstructionDefinitionList(byte op, params InstructionDefinition[] entries)
        {
            Op = op;
            if (entries != null)
                _instructions.AddRange(entries);
        }

        public void Add(InstructionDefinition enty) { _instructions.Add(enty); }

        public IEnumerator<InstructionDefinition> GetEnumerator() => _instructions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
