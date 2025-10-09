using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using InventorySystem2.Views;

namespace InventorySystem2;

public partial class App : Application
{
    public override void Initialize() 
        => AvaloniaXamlLoader.Load(this); // loader alt xaml indhold ved start

    public override void OnFrameworkInitializationCompleted()
    {
        // tjek at vi kører som desktop-app
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow(); // sætter hovedvinduet

        base.OnFrameworkInitializationCompleted(); // kør base metode bagefter
    }
}