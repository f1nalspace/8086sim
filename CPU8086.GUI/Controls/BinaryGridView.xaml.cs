using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Final.CPU8086.Controls
{
    /// <summary>
    /// Interaction logic for BinaryGridView.xaml
    /// </summary>
    public partial class BinaryGridView : UserControl, IAutoService
    {
        public static readonly DependencyProperty StreamSourceProperty =
            DependencyProperty.Register(nameof(StreamSource), typeof(object), typeof(BinaryGridView), new PropertyMetadata(null, StreamSourcePropertyChanged));
        private static void StreamSourcePropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            if (element is BinaryGridView gridView)
            {
                BinaryGridViewModel vm = gridView.ViewModel;
                vm.LoadStreamFromSource(args.NewValue);
            }
        }

        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.Register(nameof(SelectionStart), typeof(uint), typeof(BinaryGridView), new PropertyMetadata(0U, SelectionStartPropertyChanged));
        private static void SelectionStartPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            if (element is BinaryGridView gridView)
            {
                BinaryGridViewModel vm = gridView.ViewModel;
                vm.SelectionStart = (uint)args.NewValue;
            }
        }

        public static readonly DependencyProperty SelectionLengthProperty =
            DependencyProperty.Register(nameof(SelectionLength), typeof(uint), typeof(BinaryGridView), new PropertyMetadata(0U, SelectionLengthPropertyChanged));
        private static void SelectionLengthPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            if (element is BinaryGridView gridView)
            {
                BinaryGridViewModel vm = gridView.ViewModel;
                vm.SelectionLength = (uint)args.NewValue;
            }
        }

        public static readonly DependencyProperty ShowAsHexProperty =
            DependencyProperty.Register(nameof(ShowAsHex), typeof(bool), typeof(BinaryGridView), new PropertyMetadata(false, ShowAsHexPropertyChanged));
        private static void ShowAsHexPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            if (element is BinaryGridView gridView)
            {
                BinaryGridViewModel vm = gridView.ViewModel;
                vm.ShowAsHex = (bool)args.NewValue;
            }
        }

        public static readonly DependencyProperty BytesPerPageProperty =
            DependencyProperty.Register(nameof(BytesPerPage), typeof(uint), typeof(BinaryGridView), new PropertyMetadata(0U, BytesPerPagePropertyChanged));
        private static void BytesPerPagePropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            if (element is BinaryGridView gridView)
            {
                BinaryGridViewModel vm = gridView.ViewModel;
                vm.BytesPerPage = (uint)args.NewValue;
            }
        }

        public static readonly DependencyPropertyKey CanFirstPagePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CanFirstPage), typeof(bool), typeof(BinaryGridView), new PropertyMetadata(false));
        public static readonly DependencyProperty CanFirstPageProperty = CanFirstPagePropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey CanLastPagePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CanLastPage), typeof(bool), typeof(BinaryGridView), new PropertyMetadata(false));
        public static readonly DependencyProperty CanLastPageProperty = CanLastPagePropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey CanNextPagePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CanNextPage), typeof(bool), typeof(BinaryGridView), new PropertyMetadata(false));
        public static readonly DependencyProperty CanNextPageProperty = CanNextPagePropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey CanPrevPagePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CanPrevPage), typeof(bool), typeof(BinaryGridView), new PropertyMetadata(false));
        public static readonly DependencyProperty CanPrevPageProperty = CanPrevPagePropertyKey.DependencyProperty;

        public object StreamSource
        {
            get => GetValue(StreamSourceProperty);
            set => SetCurrentValue(StreamSourceProperty, value);
        }

        public uint SelectionStart
        {
            get => (uint)GetValue(SelectionStartProperty);
            set => SetCurrentValue(SelectionStartProperty, value);
        }
        public uint SelectionLength
        {
            get => (uint)GetValue(SelectionLengthProperty);
            set => SetCurrentValue(SelectionLengthProperty, value);
        }

        public bool ShowAsHex
        {
            get => (bool)GetValue(ShowAsHexProperty);
            set => SetCurrentValue(ShowAsHexProperty, value);
        }

        public uint BytesPerPage
        {
            get => (uint)GetValue(BytesPerPageProperty);
            set => SetCurrentValue(BytesPerPageProperty, value);
        }

        public bool CanFirstPage
        {
            get => (bool)GetValue(CanFirstPageProperty);
            private set => SetValue(CanFirstPagePropertyKey, value);
        }

        public bool CanLastPage
        {
            get => (bool)GetValue(CanLastPageProperty);
            private set => SetValue(CanLastPagePropertyKey, value);
        }

        public bool CanNextPage
        {
            get => (bool)GetValue(CanNextPageProperty);
            private set => SetValue(CanNextPagePropertyKey, value);
        }

        public bool CanPrevPage
        {
            get => (bool)GetValue(CanPrevPageProperty);
            private set => SetValue(CanPrevPagePropertyKey, value);
        }

        public BinaryGridViewModel ViewModel => mainGrid?.DataContext as BinaryGridViewModel;

        public BinaryGridView()
        {
            InitializeComponent();

            mainGrid.DataContextChanged += OnMainGridDataContextChanged;
        }

        private void OnMainGridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is BinaryGridViewModel oldVM)
                oldVM.PropertyChanged -= OnViewModelPropertyChanged;
            if (e.NewValue is BinaryGridViewModel newVM)
                newVM.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(BinaryGridViewModel.CanFirstPage).Equals(e.PropertyName))
                CanFirstPage = ViewModel.CanFirstPage;
            else if (nameof(BinaryGridViewModel.CanLastPage).Equals(e.PropertyName))
                CanLastPage = ViewModel.CanLastPage;
            else if (nameof(BinaryGridViewModel.CanNextPage).Equals(e.PropertyName))
                CanNextPage = ViewModel.CanNextPage;
            else if (nameof(BinaryGridViewModel.CanPrevPage).Equals(e.PropertyName))
                CanPrevPage = ViewModel.CanPrevPage;
        }

        public IAutoService GetAutoService() => ViewModel;
    }
}
