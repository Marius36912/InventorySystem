using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using InventorySystem2.Models;

namespace InventorySystem2.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Inventory _inventory;   // lagerobjekt
    private readonly OrderBook _orderBook;   // ordrebog

    public ObservableCollection<Order> QueuedOrders { get; }     // kø
    public ObservableCollection<Order> ProcessedOrders { get; }  // færdige

    private decimal _totalRevenue;  // samlet omsætning
    public decimal TotalRevenue
    {
        get => _totalRevenue;
        private set { _totalRevenue = value; OnPropertyChanged(); } // opdater ui
    }

    public ICommand ProcessNextCommand { get; }  // knapkommando

    public MainWindowViewModel()
    {
        // opretter nogle testvarer eksempler
        var rice  = new BulkItem("Rice", 1.20m, "kg");
        var cable = new BulkItem("Cable", 4.00m, "m");
        var screw = new UnitItem("Screw", 0.50m, 0.02);
        var pen   = new UnitItem("Pen", 2.00m, 0.01);

        // startlager
        var stock = new Dictionary<Item, double>
        {
            [rice] = 12.0, [cable] = 40.0, [screw] = 50.0, [pen] = 3.0
        };

        _inventory = new Inventory(stock);
        _orderBook = new OrderBook();

        // laver tre ordrer bare som eksempel
        var o1 = new Order(DateTime.Now.AddMinutes(-10), new()
        {
            new OrderLine(rice, 2.5),
            new OrderLine(pen, 2)
        });
        var o2 = new Order(DateTime.Now.AddMinutes(-7), new()
        {
            new OrderLine(screw, 10),
            new OrderLine(cable, 3)
        });
        var o3 = new Order(DateTime.Now.AddMinutes(-2), new()
        {
            new OrderLine(pen, 1),
            new OrderLine(rice, 1.0)
        });

        // sæt i kø
        _orderBook.QueueOrder(o1);
        _orderBook.QueueOrder(o2);
        _orderBook.QueueOrder(o3);

        // vis i ui
        QueuedOrders    = new ObservableCollection<Order>(_orderBook.QueuedOrders);
        ProcessedOrders = new ObservableCollection<Order>(_orderBook.ProcessedOrders);

        TotalRevenue = _orderBook.TotalRevenue();

        // kommando til knappen
        ProcessNextCommand = new RelayCommand(_ => ProcessNext(),
                                              _ => QueuedOrders.Count > 0);

        // opdater knapstatus ved ændring
        QueuedOrders.CollectionChanged += (_, __)
            => ((RelayCommand)ProcessNextCommand).RaiseCanExecuteChanged();
    }

    private void ProcessNext()
    {
        var processed = _orderBook.ProcessNextOrder(_inventory);
        if (processed is null) return;

        // fjern fra kø og tilføj til færdige
        if (QueuedOrders.Count > 0) QueuedOrders.RemoveAt(0);
        ProcessedOrders.Add(processed);

        // opdater omsætning
        TotalRevenue = _orderBook.TotalRevenue();
    }

    // event til property changed
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// enkel i command til knappen
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;          // metode
    private readonly Func<object?, bool>? _canExecute;  // betingelse

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute; _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;  // om knap må trykkes
    public void Execute(object? parameter) => _execute(parameter);  // udfør handling
    public event EventHandler? CanExecuteChanged;  // hændelse til opdatering
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);  // tving opdatering
}
