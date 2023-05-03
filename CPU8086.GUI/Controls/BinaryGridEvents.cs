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
        public uint PageCount { get; }
        public uint PageOffset { get; }

        public BinaryGridPageChangedEventArgs(BinaryGridViewModel view, uint pageCount, uint pageOffset) : base(view)
        {
            PageCount = pageCount;
            PageOffset = pageOffset;
        }

        public BinaryGridPageChangedEventArgs(RoutedEvent routedEvent, object source, BinaryGridViewModel view, uint pageCount, uint pageOffset) : base(routedEvent, source, view)
        {
            PageCount = pageCount;
            PageOffset = pageOffset;
        }
    }

    public delegate void BinaryGridPageChangedEventHandler(object sender, BinaryGridPageChangedEventArgs args);
}
