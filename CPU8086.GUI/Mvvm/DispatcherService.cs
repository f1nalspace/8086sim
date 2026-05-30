using Avalonia.Threading;
using System;

// Kompatibilitaets-Shim fuer DevExpress.Mvvm.IDispatcherService.
// Wrappt Avalonias UI-Thread-Dispatcher. Wird in der View an den ServiceContainer
// des ViewModels gereicht (vgl. frueher dxmvvm:DispatcherService im XAML).
namespace Final.CPU8086.Mvvm;

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

// Stub-Dispatcher: fuehrt die Action direkt aus. Dient als Fallback, wenn (noch)
// kein UI-Dispatcher registriert ist, damit Aufrufer keine null-Pruefung brauchen.
public sealed class DirectDispatcherService : IDispatcherService
{
    public static readonly DirectDispatcherService Instance = new DirectDispatcherService();

    public void Invoke(Action action) => action?.Invoke();
}