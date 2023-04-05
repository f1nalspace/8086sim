using System.Collections;
using System.Collections.Generic;

namespace Final.CPU8086
{
    public class InstructionList : IReadOnlyCollection<InstructionEntry>
    {
        private readonly List<InstructionEntry> _instructions = new List<InstructionEntry>();

        public byte Op { get; }

        public int Count => _instructions.Count;

        public InstructionList(byte op, params InstructionEntry[] entries)
        {
            Op = op;
            if (entries != null)
                _instructions.AddRange(entries);
        }

        public void Add(InstructionEntry enty) { _instructions.Add(enty); }

        public IEnumerator<InstructionEntry> GetEnumerator() => _instructions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
