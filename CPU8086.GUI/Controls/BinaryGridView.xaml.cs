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
    public partial class BinaryGridView : UserControl
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

        public BinaryGridViewModel ViewModel => mainGrid?.DataContext as BinaryGridViewModel;

        public BinaryGridView()
        {
            InitializeComponent();
        }
    }
}
