using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows.Controls;

namespace Final.CPU8086
{
    public abstract class AutoServiceBehavior<T> : Behavior<T>
        where T : UserControl, IAutoService
    {
        public string Key { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();

            var dt = AssociatedObject?.DataContext;
            if (dt is ISupportServices supportServices)
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
            var dt = AssociatedObject?.DataContext;
            if (dt is ISupportServices supportServices)
            {
                IAutoService service = AssociatedObject.GetAutoService();
                supportServices.ServiceContainer.UnregisterService(service);
            }
            base.OnDetaching();
        }
    }
}
