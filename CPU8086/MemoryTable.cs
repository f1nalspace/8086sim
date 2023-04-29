using System;
using System.Collections.Immutable;
using System.Linq;

namespace Final.CPU8086
{
    public class MemoryTable
    {
        private readonly byte[] _raw = new byte[0xFFFFF];

        public const int PageSize = 0xFF;

        public int Length => _raw.Length;
        public int PageCount => Length / PageSize;

        public byte this[uint index]
        {
            get => _raw[index];
            set => _raw[index] = value;
        }

        public MemoryTable()
        {
            Array.Clear(_raw, 0, _raw.Length);
        }

        public void Clear()
        {
            Array.Clear(_raw, 0, _raw.Length);
        }

        public void Set(uint offset, ReadOnlySpan<byte> data)
        {
            if (offset + data.Length > _raw.Length)
                throw new InsufficientMemoryException($"Can´t load '{data.Length}' bytes of memory starting from '{offset}'");
            for (int i = 0; i < data.Length; ++i)
                _raw[offset + i] = data[i];
        }

        public ReadOnlySpan<byte> Get(int offset, int length) => _raw.AsSpan(offset, length);

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
