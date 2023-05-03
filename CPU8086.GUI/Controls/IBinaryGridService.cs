using System.ComponentModel;

namespace Final.CPU8086.Controls
{
    public interface IBinaryGridService : INotifyPropertyChanged, IAutoService
    {
        bool ShowAsHex { get; set; }
        uint SelectionStart { get; set; }
        uint SelectionLength { get; set; }
        uint BytesPerPage { get; set; }
        uint PageCount { get; }
        uint PageOffset { get; }

        StreamByte[] Stream { get; }
        StreamByte[] Page { get; }

        bool AllowPaging { get; }
     
        bool CanFirstPage { get; }
        void FirstPage();
        bool CanLastPage { get; }
        void LastPage();
        bool CanNextPage { get; }
        void NextPage();
        bool CanPrevPage { get; }
        void PrevPage();

        void PageFromByte(uint byteIndex);

        event BinaryGridCellClickEventHandler CellClicked;
        event BinaryGridPageChangedEventHandler PageChanged;
    }
}
