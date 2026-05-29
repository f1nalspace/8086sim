using Avalonia;
using Avalonia.Controls;
using System.ComponentModel;

namespace Final.CPU8086.Controls
{
    public partial class BinaryGridView : UserControl, IAutoService
    {
        // --- nach innen schreibende StyledProperties (bruecken aeussere Bindings auf das Grid-VM) ---

        public static readonly StyledProperty<object> StreamSourceProperty =
            AvaloniaProperty.Register<BinaryGridView, object>(nameof(StreamSource));

        public static readonly StyledProperty<uint> SelectionStartProperty =
            AvaloniaProperty.Register<BinaryGridView, uint>(nameof(SelectionStart));

        public static readonly StyledProperty<uint> SelectionLengthProperty =
            AvaloniaProperty.Register<BinaryGridView, uint>(nameof(SelectionLength));

        public static readonly StyledProperty<bool> ShowAsHexProperty =
            AvaloniaProperty.Register<BinaryGridView, bool>(nameof(ShowAsHex));

        public static readonly StyledProperty<uint> BytesPerPageProperty =
            AvaloniaProperty.Register<BinaryGridView, uint>(nameof(BytesPerPage));

        public object StreamSource
        {
            get => GetValue(StreamSourceProperty);
            set => SetValue(StreamSourceProperty, value);
        }

        public uint SelectionStart
        {
            get => GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public uint SelectionLength
        {
            get => GetValue(SelectionLengthProperty);
            set => SetValue(SelectionLengthProperty, value);
        }

        public bool ShowAsHex
        {
            get => GetValue(ShowAsHexProperty);
            set => SetValue(ShowAsHexProperty, value);
        }

        public uint BytesPerPage
        {
            get => GetValue(BytesPerPageProperty);
            set => SetValue(BytesPerPageProperty, value);
        }

        // --- ReadOnly DirectProperties (aus dem VM gespeist) ---

        public static readonly DirectProperty<BinaryGridView, bool> CanFirstPageProperty =
            AvaloniaProperty.RegisterDirect<BinaryGridView, bool>(nameof(CanFirstPage), o => o.CanFirstPage);
        public static readonly DirectProperty<BinaryGridView, bool> CanLastPageProperty =
            AvaloniaProperty.RegisterDirect<BinaryGridView, bool>(nameof(CanLastPage), o => o.CanLastPage);
        public static readonly DirectProperty<BinaryGridView, bool> CanNextPageProperty =
            AvaloniaProperty.RegisterDirect<BinaryGridView, bool>(nameof(CanNextPage), o => o.CanNextPage);
        public static readonly DirectProperty<BinaryGridView, bool> CanPrevPageProperty =
            AvaloniaProperty.RegisterDirect<BinaryGridView, bool>(nameof(CanPrevPage), o => o.CanPrevPage);

        private bool _canFirstPage;
        private bool _canLastPage;
        private bool _canNextPage;
        private bool _canPrevPage;

        public bool CanFirstPage
        {
            get => _canFirstPage;
            private set => SetAndRaise(CanFirstPageProperty, ref _canFirstPage, value);
        }
        public bool CanLastPage
        {
            get => _canLastPage;
            private set => SetAndRaise(CanLastPageProperty, ref _canLastPage, value);
        }
        public bool CanNextPage
        {
            get => _canNextPage;
            private set => SetAndRaise(CanNextPageProperty, ref _canNextPage, value);
        }
        public bool CanPrevPage
        {
            get => _canPrevPage;
            private set => SetAndRaise(CanPrevPageProperty, ref _canPrevPage, value);
        }

        private readonly BinaryGridViewModel _viewModel = new BinaryGridViewModel();
        public BinaryGridViewModel ViewModel => _viewModel;

        public BinaryGridView()
        {
            InitializeComponent();
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            mainGrid.DataContext = _viewModel;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (_viewModel == null)
                return;

            if (change.Property == StreamSourceProperty)
                _viewModel.LoadStreamFromSource(change.NewValue);
            else if (change.Property == SelectionStartProperty)
                _viewModel.SelectionStart = change.GetNewValue<uint>();
            else if (change.Property == SelectionLengthProperty)
                _viewModel.SelectionLength = change.GetNewValue<uint>();
            else if (change.Property == ShowAsHexProperty)
                _viewModel.ShowAsHex = change.GetNewValue<bool>();
            else if (change.Property == BytesPerPageProperty)
                _viewModel.BytesPerPage = change.GetNewValue<uint>();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (nameof(BinaryGridViewModel.CanFirstPage).Equals(e.PropertyName))
                CanFirstPage = _viewModel.CanFirstPage;
            else if (nameof(BinaryGridViewModel.CanLastPage).Equals(e.PropertyName))
                CanLastPage = _viewModel.CanLastPage;
            else if (nameof(BinaryGridViewModel.CanNextPage).Equals(e.PropertyName))
                CanNextPage = _viewModel.CanNextPage;
            else if (nameof(BinaryGridViewModel.CanPrevPage).Equals(e.PropertyName))
                CanPrevPage = _viewModel.CanPrevPage;
        }

        public IAutoService GetAutoService() => _viewModel;
    }
}
