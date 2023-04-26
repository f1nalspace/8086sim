using System.ComponentModel;

namespace Final.CPU8086.Controls
{
    public interface IBinaryGridService : INotifyPropertyChanged
    {
        bool ShowAsHex { get; set; }
        uint SelectionStart { get; set; }
        uint SelectionLength { get; set; }
        uint BytesPerPage { get; set; }
        uint PageCount { get; }
        uint PageOffset { get; }

        StreamByte[] Stream { get; }
        StreamByte[] Page { get; }

        void FirstPage();
        void LastPage();
        void NextPage();
        void PrevPage();
        void PageFromByte(uint byteIndex);

        event BinaryGridCellClickEventHandler CellClicked;
    }
}
