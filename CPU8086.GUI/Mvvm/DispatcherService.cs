using Avalonia.Threading;
using System;

// Kompatibilitaets-Shim fuer DevExpress.Mvvm.IDispatcherService.
// Wrappt Avalonias UI-Thread-Dispatcher. Wird in der View an den ServiceContainer
// des ViewModels gereicht (vgl. frueher dxmvvm:DispatcherService im XAML).
namespace DevExpress.Mvvm
{
    public interface IDispatcherService
    {
        void Invoke(Action action);
    }

    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            if (action == null)
                return;
            if (Dispatcher.UIThread.CheckAccess())
                action();
            else
                Dispatcher.UIThread.Invoke(action);
        }
    }
}
