using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Final.CPU8086.Controls;
using Final.CPU8086.Mvvm;
using Final.CPU8086.Services;

namespace Final.CPU8086;

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

    // The instructions grid is a read-only display: keep the selection pinned to the current
    // execution row (driven by the SelectedItem binding) and snap back any manual row clicks.
    private void OnInstructionsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        object current = _viewModel.CurrentStreamInstruction;
        if (sender is DataGrid grid && !Equals(grid.SelectedItem, current))
            grid.SelectedItem = current;
    }

    // Pure view concern: flip the application-wide Light/Dark theme variant.
    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        bool dark = (sender as ToggleButton)?.IsChecked == true;
        if (Application.Current is { } app)
            app.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
    }
}