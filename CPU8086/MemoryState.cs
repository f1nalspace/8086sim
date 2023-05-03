using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Final.CPU8086
{
    public class MemoryState : IReadOnlyCollection<byte>
    {
        private readonly byte[] _data = new byte[0xFFFFF];

        public const int PageSize = 0xFF;

        public int Length => _data.Length;
        public int PageCount => Length / PageSize;
        public int Count => _data.Length;

        public byte this[uint index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public MemoryState()
        {
            Array.Clear(_data, 0, _data.Length);
        }

        public void Clear()
        {
            Array.Clear(_data, 0, _data.Length);
        }

        public void Set(uint offset, ReadOnlySpan<byte> data)
        {
            if (offset + data.Length > _data.Length)
                throw new InsufficientMemoryException($"Can´t load '{data.Length}' bytes of memory starting from '{offset}'");
            for (int i = 0; i < data.Length; ++i)
                _data[offset + i] = data[i];
        }

        public ReadOnlySpan<byte> Get(uint offset, uint length) => _data.AsSpan((int)offset, (int)length);

        public ImmutableArray<byte> ReadPage(int pageIndex)
        {
            int index = pageIndex * PageSize;
            if (index < 0 || index >= _data.Length)
                return ImmutableArray<byte>.Empty;
            int len = Math.Min(_data.Length - index, PageSize);
            return _data.AsSpan(index, len).ToArray().ToImmutableArray();
        }

        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_data).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    }
}
