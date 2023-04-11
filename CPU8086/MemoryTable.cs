using System;
using System.Collections.Immutable;
using System.Linq;

namespace Final.CPU8086
{
    public class MemoryTable
    {
        private readonly byte[] _raw = new byte[0xFFFFF];
        private Memory<byte> _mem;

        public const int PageSize = 0xFF;

        public int Length => _raw.Length;
        public int PageCount => Length / PageSize;

        public byte this[int index]
        {
            get => _raw[index];
            set => _raw[index] = value;
        }

        public MemoryTable()
        {
            _mem = new Memory<byte>(_raw);
            Array.Clear(_raw, 0, _raw.Length);
        }

        public void Clear()
        {
            Array.Clear(_raw, 0, _raw.Length);
        }

        public ImmutableArray<byte> ReadPage(int pageIndex)
        {
            int index = pageIndex * PageSize;
            if (index < 0 || index >= _raw.Length)
                return ImmutableArray<byte>.Empty;
            int len = Math.Min(_raw.Length - index, PageSize);
            return _raw.AsSpan(index, len).ToArray().ToImmutableArray();
        }
    }
}
