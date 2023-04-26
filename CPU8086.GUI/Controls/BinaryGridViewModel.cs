using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;

namespace Final.CPU8086.Controls
{
    public class BinaryGridViewModel : ViewModelBase, IBinaryGridService
    {
        public const int ColumnCount = 8;

        public bool ShowAsHex
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public uint SelectionStart
        {
            get => GetValue<uint>();
            set => SetValue(value);
        }

        public uint SelectionLength
        {
            get => GetValue<uint>();
            set => SetValue(value);
        }

        public uint BytesPerPage
        {
            get => GetValue<uint>();
            set => SetValue(value, () => BytesPerPageChanged(Stream, value));
        }

        public uint PageCount
        {
            get => GetValue<uint>();
            private set => SetValue(value);
        }

        public uint PageOffset
        {
            get => GetValue<uint>();
            set => SetValue(value, () => PageOffsetChanged(Stream, value, PageCount, BytesPerPage));
        }

        public uint StreamStart
        {
            get => GetValue<uint>();
            private set => SetValue(value);
        }

        public StreamByte[] Stream
        {
            get => _stream;
            set => SetValue(ref _stream, value, () => CurrentStreamChanged(value));
        }
        private StreamByte[] _stream = Array.Empty<StreamByte>();

        public ImmutableArray<uint> Lines
        {
            get => _lines;
            set => SetValue(ref _lines, value);
        }
        private ImmutableArray<uint> _lines = ImmutableArray<uint>.Empty;

        public StreamByte[] Page
        {
            get => _page;
            private set => SetValue(ref _page, value);
        }
        private StreamByte[] _page = Array.Empty<StreamByte>();

        public event BinaryGridCellClickEventHandler CellClicked;

        public DelegateCommand<StreamByte> CellClickCommand { get; }

        public BinaryGridViewModel()
        {
            CellClickCommand = new DelegateCommand<StreamByte>(CellClick);

            //StreamByte[] r = new StreamByte[256];
            //for (uint i = 0; i < 256; i++)
            //    r[i] = new StreamByte(i, (byte)i);
            //Stream = r;

            //SelectionStart = 9;
            //SelectionLength = 3;

            //BytesPerPage = 64;
            //PageOffset = 0;
        }

        private void BytesPerPageChanged(StreamByte[] stream, uint bytesPerPage)
        {
            if (bytesPerPage > 0 && stream.Length > 0)
            {
                PageCount = (uint)Math.Ceiling(_stream.Length / (double)bytesPerPage);
                PageOffset = 0;
            }
            else
            {
                PageCount = 0;
                PageOffset = 0;
            }

            PageOffsetChanged(stream, PageOffset, PageCount, bytesPerPage);
        }

        private void UpdateLines(uint byteOffset, uint byteCount)
        {
            if (byteCount > 0)
            {
                uint lineCount = (uint)Math.Ceiling(byteCount / (double)ColumnCount);
                uint[] lines = new uint[lineCount];
                for (uint i = 0; i < lineCount; ++i)
                    lines[i] = byteOffset + (uint)(i * ColumnCount);
                Lines = lines.ToImmutableArray();
            }
            else
                Lines = ImmutableArray<uint>.Empty;
        }

        private void CurrentStreamChanged(StreamByte[] stream)
        {
            BytesPerPageChanged(stream, BytesPerPage);

            SelectionStart = 0;
            SelectionLength = 0;
        }

        private void PageOffsetChanged(StreamByte[] stream, uint pageOffset, uint pageCount, uint bytesPerPage)
        {
            int streamLength = stream.Length;
            if (streamLength > 0 && pageCount > 0 && bytesPerPage > 0)
            {
                Contract.Assert(pageCount > 0);
                uint byteIndex = pageCount > 0 ? Math.Max(0, Math.Min(pageOffset * bytesPerPage, pageCount - 1)) : 0;

                uint byteCount = Math.Min(BytesPerPage, (uint)(streamLength - byteIndex));
                Page = stream
                    .AsSpan()
                    .Slice((int)byteIndex, (int)byteCount)
                    .ToArray();

                StreamStart = (uint)byteIndex;

                UpdateLines((uint)byteIndex, (uint)byteCount);
            }
            else
            {
                Page = stream.ToArray();
                StreamStart = 0;
                UpdateLines(0, (uint)stream.Length);
            }
        }

        public void FirstPage() => PageOffset = 0;
        public void LastPage() => PageOffset = PageCount > 0 ? PageCount - 1 : 0;
        public void NextPage()
        {
            if (PageCount == 0 || BytesPerPage == 0)
                return;
            if (PageOffset < (PageCount - 1))
                PageOffset++;
        }
        public void PrevPage()
        {
            if (PageCount == 0 || BytesPerPage == 0)
                return;
            if (PageOffset > 0)
                PageOffset--;
        }
        public void PageFromByte(uint byteIndex)
        {
            if (PageCount == 0 || BytesPerPage == 0)
                return;
            PageOffset = byteIndex / BytesPerPage;
        }

        private void CellClick(StreamByte cell) => CellClicked?.Invoke(this, new BinaryGridCellClickEventArgs(this, cell));

        public void LoadStreamFromSource(object source)
        {
            if (source is IEnumerable<byte> bytes)
            {
                uint count = 0;
                Stream = bytes
                    .Select(b => new StreamByte(Interlocked.Increment(ref count) - 1, b))
                    .ToArray();
            }
            else
                Stream = Array.Empty<StreamByte>();
        }
    }
}
