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
}
