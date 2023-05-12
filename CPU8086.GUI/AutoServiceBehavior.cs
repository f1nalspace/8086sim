using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;

namespace Final.CPU8086
{
    public abstract class AutoServiceBehavior<T> : Behavior<T>
        where T : UserControl, IAutoService
    {
        public string Key { get; set; }

        protected ISupportServices GetSupportServices(FrameworkElement obj)
        {
            object dt = obj?.DataContext;
            if (dt is ISupportServices supportServices)
                return supportServices;
            return null;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            ISupportServices supportServices = GetSupportServices(AssociatedObject);
            if (supportServices != null)
            {
                IAutoService service = AssociatedObject.GetAutoService();
                if (!string.IsNullOrEmpty(Key))
                    supportServices.ServiceContainer.RegisterService(Key, service);
                else
                    supportServices.ServiceContainer.RegisterService(service);
            }
        }

        protected override void OnDetaching()
        {
            ISupportServices supportServices = GetSupportServices(AssociatedObject);
            if (supportServices != null)
            {
                IAutoService service = AssociatedObject.GetAutoService();
                supportServices.ServiceContainer.UnregisterService(service);
            }
            base.OnDetaching();
        }
    }
}
