using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using Final.CPU8086.Types;
using Final.CPU8086.Services;

namespace Final.CPU8086.Controls
{
    public class BinaryGridViewModel : ViewModelBase, IBinaryGridService
    {
        public const int ColumnCount = 8;

        private IMemoryAddressResolverService MemoryAddressResolver => GetService<IMemoryAddressResolverService>();

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
            private set => SetValue(value, () => PageCountChanged(value));
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

        public uint VisualPosition
        {
            get => GetValue<uint>();
            private set => SetValue(value);
        }

        public uint VisualLength
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
        public event BinaryGridPageChangedEventHandler PageChanged;
        public event BinaryGridPageChangedEventHandler PageChanging;
        //public event BinaryGridResolveAddressEventHandler ResolveMemoryAddress;

        public string JumpAddress
        {
            get => _jumpAddress;
            set => SetValue(ref _jumpAddress, value);
        }
        private string _jumpAddress = null;

        public DelegateCommand<StreamByte> CellClickCommand { get; }
        public DelegateCommand<string> JumpToAddressCommand { get; }

        public BinaryGridViewModel()
        {
            CellClickCommand = new DelegateCommand<StreamByte>(CellClick);
            JumpToAddressCommand = new DelegateCommand<string>(JumpToAddress, CanJumpToAddress);

            //StreamByte[] r = new StreamByte[256];
            //for (uint i = 0; i < 256; i++)
            //    r[i] = new StreamByte(i, (byte)i);
            //Stream = r;

            //SelectionStart = 9;
            //SelectionLength = 3;

            //BytesPerPage = 64;
            //PageOffset = 0;
        }

        private static readonly Regex _hexRangeRex = new Regex("(?:(?<seg>(cs|CS|ds|DS|ss|SS|es|ES))\\:)?0[xX](?<first>[0-9a-fA-F]+)\\s*(?:[-]0[xX](?<second>[0-9a-fA-F]+))?", RegexOptions.Compiled);
        private static readonly Regex _intRangeRex = new Regex("(?:(?<seg>(cs|CS|ds|DS|ss|SS|es|ES))\\:)?(?<first>[0-9]+)\\s*(?:[-](?<second>[0-9]+))?", RegexOptions.Compiled);

        private static SegmentType ParseSegmentType(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return SegmentType.None;
            return str.ToLower() switch
            {
                "cs" => SegmentType.CS,
                "ds" => SegmentType.DS,
                "ss" => SegmentType.SS,
                "es" => SegmentType.ES,
                _ => SegmentType.None
            };
        }

        private static (SegmentType seg, uint Start, uint Length) GetAddressRange(string address)
        {
            SegmentType seg;
            uint start, end;
            if (!string.IsNullOrEmpty(address))
            {
                Match hexMatch = _hexRangeRex.Match(address);
                if (hexMatch.Success)
                {
                    if (!string.IsNullOrWhiteSpace(hexMatch.Groups["seg"]?.Value))
                        seg = ParseSegmentType(hexMatch.Groups["seg"]?.Value);
                    else
                        seg = SegmentType.None;
                    start = uint.Parse(hexMatch.Groups["first"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    if (!string.IsNullOrWhiteSpace(hexMatch.Groups["second"]?.Value))
                        end = uint.Parse(hexMatch.Groups["second"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    else
                        end = start;
                }
                else
                {
                    Match intMatch = _intRangeRex.Match(address);
                    if (intMatch.Success)
                    {
                        if (!string.IsNullOrWhiteSpace(hexMatch.Groups["seg"]?.Value))
                            seg = ParseSegmentType(hexMatch.Groups["seg"]?.Value);
                        else
                            seg = SegmentType.None;
                        start = uint.Parse(intMatch.Groups["first"].Value);
                        if (!string.IsNullOrWhiteSpace(intMatch.Groups["second"]?.Value))
                            end = uint.Parse(intMatch.Groups["second"].Value);
                        else
                            end = start;
                    }
                    else
                        return (SegmentType.None, 0, 0);
                }
            }
            else
                return (SegmentType.None, 0, 0);
            if (start > end)
                return (SegmentType.None, 0, 0);
            else if (start == end)
                return (seg, start, 1);
            return (seg, start, (end - start) + 1);
        }

        private bool CanJumpToAddress(string address) => true;
        private void JumpToAddress(string address)
        {
            (SegmentType Seg, uint Start, uint Length) range = GetAddressRange(address);
            if (range.Length == 0)
            {
                VisualPosition = 0;
                VisualLength = 0;
                return;
            }

            Contract.Assert(range.Length > 0);

            uint start = range.Start;

            IMemoryAddressResolverService srv = MemoryAddressResolver;
            if (srv != null)
                start = srv.Resolve(range.Seg, (int)range.Start);

            if (PageCount > 0 && BytesPerPage > 0)
                PageOffset = Math.Max(0, Math.Min(PageCount - 1, start / BytesPerPage));
            else
                PageOffset = 0;

            VisualPosition = range.Start;
            VisualLength = range.Length;
        }

        private void RefreshPagingProperties()
        {
            RaisePropertiesChanged(nameof(CanFirstPage), nameof(CanLastPage), nameof(CanNextPage), nameof(CanPrevPage));
        }

        private void PageCountChanged(uint count)
        {
            RefreshPagingProperties();
            PageChanged?.Invoke(this, new BinaryGridPageChangedEventArgs(this, PageOffset, count, BytesPerPage));
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

        private void ReloadPage(StreamByte[] stream, uint pageOffset, uint pageCount, uint bytesPerPage)
        {
            int streamLength = stream.Length;
            if (streamLength > 0 && pageCount > 0 && bytesPerPage > 0)
            {
                uint byteIndex = Math.Max(0, Math.Min(pageOffset, pageCount - 1)) * bytesPerPage;

                uint byteCount = Math.Min(bytesPerPage, (uint)(streamLength - byteIndex));

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

        private void PageOffsetChanged(StreamByte[] stream, uint pageOffset, uint pageCount, uint bytesPerPage)
        {
            PageChanging?.Invoke(this, new BinaryGridPageChangedEventArgs(this, pageOffset, pageCount, bytesPerPage));
            ReloadPage(stream, pageOffset, pageCount, bytesPerPage);
            RefreshPagingProperties();
            PageChanged?.Invoke(this, new BinaryGridPageChangedEventArgs(this, pageOffset, pageCount, bytesPerPage));
        }

        public bool AllowPaging => PageCount > 0 && BytesPerPage > 0;
        public bool CanFirstPage => AllowPaging && PageOffset > 0;
        public void FirstPage() => PageOffset = 0;
        public bool CanLastPage => AllowPaging && PageOffset < PageCount - 1;
        public void LastPage() => PageOffset = PageCount > 0 ? PageCount - 1 : 0;
        public bool CanNextPage => AllowPaging && PageOffset < PageCount - 1;
        public void NextPage()
        {
            if (PageOffset < (PageCount - 1))
                PageOffset++;
        }
        public bool CanPrevPage => AllowPaging && PageOffset > 0;
        public void PrevPage()
        {
            if (PageOffset > 0)
                PageOffset--;
        }

        public void PageFromByte(uint byteIndex)
        {
            if (AllowPaging)
                PageOffset = Math.Min(Math.Max(0, byteIndex / BytesPerPage), PageCount - 1);
            else
                PageOffset = 0;
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

        public void ReloadStream(ReadOnlySpan<byte> stream, uint offset)
        {
            if (offset + stream.Length <= Stream.Length)
            {
                for (uint index = offset; index < offset + stream.Length; ++index)
                    Stream[index] = new StreamByte(index, stream[(int)(index - offset)]);
            }
            else
            {
                uint count = 0;
                Stream = stream.ToArray().Select(b => new StreamByte(Interlocked.Increment(ref count) - 1, b))
                    .ToArray();
            }
            ReloadPage(Stream, PageOffset, PageCount, BytesPerPage);
        }

        public (uint Offset, uint Length) ComputePageRange(uint pageOffset, uint pageCount, uint bytesPerPage)
        {
            uint streamLen = (uint)Stream.Length;
            if (streamLen > 0 && pageCount > 0 && bytesPerPage > 0)
            {
                uint byteIndex = Math.Max(0, Math.Min(pageOffset, pageCount - 1)) * bytesPerPage;
                uint byteCount = Math.Min(bytesPerPage, streamLen - byteIndex);
                return (byteIndex, byteCount);
            }
            return (0, streamLen);
        }

        public IAutoService GetAutoService() => throw new NotSupportedException();
    }
}
