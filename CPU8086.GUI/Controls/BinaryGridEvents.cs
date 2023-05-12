using Final.CPU8086.Types;
using System;
using System.Windows;

namespace Final.CPU8086.Controls
{
    public class BinaryGridEventArgs : RoutedEventArgs
    {
        public BinaryGridViewModel View { get; }

        public BinaryGridEventArgs(BinaryGridViewModel view) : base()
        {
            View = view;
        }

        public BinaryGridEventArgs(RoutedEvent routedEvent, object source, BinaryGridViewModel view) : base(routedEvent, source)
        {
            View = view;
        }
    }

    public class BinaryGridCellClickEventArgs : BinaryGridEventArgs
    {
        public StreamByte Cell { get; }

        public BinaryGridCellClickEventArgs(BinaryGridViewModel view, StreamByte cell) : base(view)
        {
            Cell = cell;
        }

        public BinaryGridCellClickEventArgs(RoutedEvent routedEvent, object source, BinaryGridViewModel view, StreamByte cell) : base(routedEvent, source, view)
        {
            Cell = cell;
        }
    }

    public delegate void BinaryGridCellClickEventHandler(object sender, BinaryGridCellClickEventArgs args);

    public class BinaryGridJumpToAddressEventArgs : BinaryGridEventArgs
    {
        public uint Address { get; }
        public bool IsHandled { get; set; }

        public BinaryGridJumpToAddressEventArgs(BinaryGridViewModel view, uint address) : base(view)
        {
            Address = address;
            IsHandled = false;
        }

        public BinaryGridJumpToAddressEventArgs(RoutedEvent routedEvent, object source, BinaryGridViewModel view, uint address) : base(routedEvent, source, view)
        {
            Address = address;
            IsHandled = false;
        }
    }

    public delegate void BinaryGridJumpToAddressEventHandler(object sender, BinaryGridJumpToAddressEventArgs args);

    public class BinaryGridPageChangedEventArgs : BinaryGridEventArgs
    {
        public uint PageOffset { get; }
        public uint PageCount { get; }
        public uint BytesPerPage { get; }

        public BinaryGridPageChangedEventArgs(BinaryGridViewModel view, uint pageOffset, uint pageCount, uint bytesPerPage) : base(view)
        {
            PageOffset = pageOffset;
            PageCount = pageCount;
            BytesPerPage = bytesPerPage;
        }

        public BinaryGridPageChangedEventArgs(RoutedEvent routedEvent, object source, BinaryGridViewModel view, uint pageOffset, uint pageCount, uint bytesPerPage) : base(routedEvent, source, view)
        {
            PageOffset = pageOffset;
            PageCount = pageCount;
            BytesPerPage = bytesPerPage;
        }
    }

    public delegate void BinaryGridPageChangedEventHandler(object sender, BinaryGridPageChangedEventArgs args);

#if false
    public class BinaryGridResolveAddressEventArgs : BinaryGridEventArgs
    {
        public SegmentType Segment { get; }
        public uint Start { get; }
        public uint Length { get; }
        public uint Result { get; set; }

        public BinaryGridResolveAddressEventArgs(BinaryGridViewModel view, SegmentType segment, uint start, uint length) : base(view)
        {
            Segment = segment;
            Start = start;
            Length = length;
            Result = 0;
        }

        public BinaryGridResolveAddressEventArgs(RoutedEvent routedEvent, object source, BinaryGridViewModel view, SegmentType segment, uint start, uint length) : base(routedEvent, source, view)
        {
            Segment = segment;
            Start = start;
            Length = length;
            Result = 0;
        }
    }

    public delegate void BinaryGridResolveAddressEventHandler(object sender, BinaryGridResolveAddressEventArgs args);
#endif
}
