// Kompatibilitaets-Shim fuer DevExpress.Mvvm.UI.Interactivity.Behavior<T>.
// Basiert auf Avalonias Behavior<T> (Avalonia.Xaml.Interactivity) - API quasi identisch
// (AssociatedObject, OnAttached, OnDetaching). Erlaubt, die vorhandenen Behaviors
// unveraendert via Interaction.Behaviors im .axaml einzubinden.
namespace Final.CPU8086.Mvvm;

public abstract class Behavior<T> : Avalonia.Xaml.Interactivity.Behavior<T>
    where T : Avalonia.AvaloniaObject
{
}