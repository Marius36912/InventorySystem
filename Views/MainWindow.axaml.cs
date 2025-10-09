using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using InventorySystem2.ViewModels;

namespace InventorySystem2.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}