using Avalonia;
using DevExpress.Mvvm;
using Final.CPU8086.Services;

namespace Final.CPU8086.Controls
{
    public class BinaryGridServiceBehavior : AutoServiceBehavior<BinaryGridView>
    {
        public static readonly StyledProperty<IMemoryAddressResolverService> MemoryAddressResolverServiceProperty =
            AvaloniaProperty.Register<BinaryGridServiceBehavior, IMemoryAddressResolverService>(nameof(MemoryAddressResolverService));

        static BinaryGridServiceBehavior()
        {
            MemoryAddressResolverServiceProperty.Changed.AddClassHandler<BinaryGridServiceBehavior>(
                (behavior, e) => behavior.OnMemoryAddressResolverServiceChanged(e.NewValue as IMemoryAddressResolverService));
        }

        private void OnMemoryAddressResolverServiceChanged(IMemoryAddressResolverService service)
        {
            BinaryGridViewModel vm = AssociatedObject?.ViewModel;
            if (service != null && vm is ISupportServices supportServices)
                supportServices.ServiceContainer.RegisterService(service);
        }

        public IMemoryAddressResolverService MemoryAddressResolverService
        {
            get => GetValue(MemoryAddressResolverServiceProperty);
            set => SetValue(MemoryAddressResolverServiceProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            IMemoryAddressResolverService srv = MemoryAddressResolverService;
            if (srv != null)
            {
                BinaryGridViewModel vm = AssociatedObject.ViewModel;
                if (vm is ISupportServices supportServices)
                    supportServices.ServiceContainer.RegisterService(srv);
            }
        }

        protected override void OnDetaching()
        {
            IMemoryAddressResolverService srv = MemoryAddressResolverService;
            if (srv != null)
            {
                BinaryGridViewModel vm = AssociatedObject.ViewModel;
                if (vm is ISupportServices supportServices)
                    supportServices.ServiceContainer.UnregisterService(srv);
            }

            base.OnDetaching();
        }
    }
}
