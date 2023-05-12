using DevExpress.Mvvm;
using Final.CPU8086.Services;
using System.Windows;

namespace Final.CPU8086.Controls
{
    public class BinaryGridServiceBehavior : AutoServiceBehavior<BinaryGridView>
    {
        public static readonly DependencyProperty MemoryAddressResolverServiceProperty = DependencyProperty.Register(nameof(MemoryAddressResolverService), typeof(IMemoryAddressResolverService), typeof(BinaryGridServiceBehavior), new PropertyMetadata(null, MemoryAddressResolverServiceChanged));

        private static void MemoryAddressResolverServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BinaryGridServiceBehavior behavior)
            {
                BinaryGridView view = behavior.AssociatedObject;
                BinaryGridViewModel vm = view?.ViewModel;
                ISupportServices supportServices = vm as ISupportServices;
                if (supportServices != null)
                    supportServices.ServiceContainer.RegisterService(e.NewValue as IMemoryAddressResolverService);
            }
        }

        public IMemoryAddressResolverService MemoryAddressResolverService
        {
            get => GetValue(MemoryAddressResolverServiceProperty) as IMemoryAddressResolverService;
            set => SetCurrentValue(MemoryAddressResolverServiceProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            IMemoryAddressResolverService srv = MemoryAddressResolverService;
            if (srv != null)
            {
                BinaryGridViewModel vm = AssociatedObject.ViewModel;
                ISupportServices supportServices = vm as ISupportServices;
                if (supportServices != null)
                    supportServices.ServiceContainer.RegisterService(srv);
            }
        }

        protected override void OnDetaching()
        {
            var srv = MemoryAddressResolverService;
            if (srv != null)
            {
                BinaryGridViewModel vm = AssociatedObject.ViewModel;
                ISupportServices supportServices = vm as ISupportServices;
                if (supportServices != null)
                    supportServices.ServiceContainer.UnregisterService(srv);
            }

            base.OnDetaching();
        }
    }
}
