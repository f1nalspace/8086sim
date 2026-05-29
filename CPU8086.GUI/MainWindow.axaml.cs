using Avalonia.Controls;
using Avalonia.Interactivity;
using DevExpress.Mvvm;
using Final.CPU8086.Controls;
using Final.CPU8086.Services;

namespace Final.CPU8086
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            // Dispatcher-Service registrieren (frueher dxmvvm:DispatcherService im XAML).
            _viewModel.ServiceContainer.RegisterService(new DispatcherService());
            DataContext = _viewModel;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            // Service-Wiring fuer das Memory-Grid (frueher AutoServiceBehavior /
            // BinaryGridServiceBehavior im XAML): das Grid-VM als "MemoryGridService"
            // beim MainViewModel registrieren und den MainViewModel (Adress-Resolver)
            // beim Grid-VM registrieren.
            if (memoryGrid?.ViewModel is IBinaryGridService gridService)
            {
                _viewModel.ServiceContainer.RegisterService("MemoryGridService", gridService);
                if (gridService is ISupportServices gridSupport && _viewModel is IMemoryAddressResolverService resolver)
                    gridSupport.ServiceContainer.RegisterService(resolver);
            }

            _viewModel.OnLoadedCommand.Execute(null);
        }
    }
}
