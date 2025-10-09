using System;
using Avalonia;

namespace InventorySystem2;

internal static class Program
{
    [STAThread] // gør at ui tråden kører som single threaded apartment
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args); // starter appen med desktop

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>() // peger først på vores App klasse
            .UsePlatformDetect()       // vælger automatisk platform windows eller mac osv.
            .WithInterFont()           // bruger standard inter-font i hele ui
            .LogToTrace();             // sender log til konsol for fejl/debug
}