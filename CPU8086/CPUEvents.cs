using System;

namespace Final.CPU8086
{
    public class MemoryChangedEventArgs : EventArgs
    {
        public uint Offset { get; }
        public uint Length { get; }

        public MemoryChangedEventArgs(uint offset, uint length)
        {
            Offset = offset;
            Length = length;
        }
    }

    public delegate void MemoryChangedEventHandler(object sender, MemoryChangedEventArgs args);
}
