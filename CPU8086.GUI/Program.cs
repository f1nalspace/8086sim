using Avalonia;
using Final.CPU8086;
using System;

// Eigener Sub-Namespace, damit die Entry-Klasse `Program` nicht den Core-Typ
// `Final.CPU8086.Execution.Program` innerhalb des `Final.CPU8086`-Namespace verdeckt.
namespace Final.CPU8086.GUI
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
