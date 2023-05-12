using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;

namespace Final.CPU8086.Behaviors
{
    public abstract class AttachServiceBehavior<TControl, TService> : Behavior<TControl> 
        where TControl : DependencyObject
    {
    }
}
