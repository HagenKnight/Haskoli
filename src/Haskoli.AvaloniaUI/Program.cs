using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Threading;

namespace Haskoli.AvaloniaUI
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                using (var mutex = new Mutex(true, "ParkAccess", out bool createdNew))
                {
                    if (!createdNew)
                    {
                        // If there's an instance, stops the current execution.
                        return;
                    }
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
