using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventorySystem2.Models;

namespace InventorySystem2.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Inventory _inventory;   // lager
    private readonly OrderBook _orderBook;   // ordrebog
    private readonly Robot _robot = new Robot("localhost", 30002); // robot socket

    public ObservableCollection<Order> QueuedOrders  { get; }     // kø
    public ObservableCollection<Order> ProcessedOrders { get; }   // færdige

    private decimal _totalRevenue;
    public decimal TotalRevenue
    {
        get => _totalRevenue;
        private set { _totalRevenue = value; OnPropertyChanged(); }
    }

    public ICommand ProcessNextCommand { get; }
    public ICommand PingCommand { get; }   // Ping/vinke

    public MainWindowViewModel()
    {
        // demo-varer
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

        // tre ordrer i kø
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

        _orderBook.QueueOrder(o1);
        _orderBook.QueueOrder(o2);
        _orderBook.QueueOrder(o3);

        // bind til UI
        QueuedOrders    = new ObservableCollection<Order>(_orderBook.QueuedOrders);
        ProcessedOrders = new ObservableCollection<Order>(_orderBook.ProcessedOrders);
        TotalRevenue    = _orderBook.TotalRevenue();

        // knapper (async)
        ProcessNextCommand = new RelayCommandAsync(_ => ProcessNextAsync(),
                                                   _ => QueuedOrders.Count > 0);
        PingCommand        = new RelayCommandAsync(_ => PingRobotAsync());

        // enable/disable når kø ændres
        QueuedOrders.CollectionChanged += (_, __)
            => ((RelayCommandAsync)ProcessNextCommand).RaiseCanExecuteChanged();
    }

    // flyt fra slot -> S (simpel generator + lille pause)
    private async Task PickToS(string slot, uint itemId)
    {
        var from = slot.ToLowerInvariant() switch
        {
            "a" => RobotPositions.A,
            "b" => RobotPositions.B,
            "c" => RobotPositions.C,
            _   => RobotPositions.A
        };
        var to = RobotPositions.S;

        var program = RobotPositions.GenerateMove(from, to);
        _robot.SendProgram(program, itemId);

        await Task.Delay(300);
    }

    // "Ping robot" – tydelig vinke-bevægelse 3 gange
    public async Task PingRobotAsync()
    {
        var prog = @"
def ping():
  home = [0, -1.57, 0, -1.57, 0, 0]
  left =  p[0.25, -0.25, 0.20, 0, -3.1415, 0]
  right = p[0.25,  0.25, 0.20, 0, -3.1415, 0]
  movej(home, a=1.2, v=0.6)
  i = 0
  while (i < 3):
    movej(get_inverse_kin(left),  a=1.2, v=0.6)
    movej(get_inverse_kin(right), a=1.2, v=0.6)
    i = i + 1
  end
end
";
        _robot.SendProgram(prog, 999);
        await Task.Delay(300);
    }

    // ordre -> robot (3 hop) -> flyt i UI -> opdater revenue
    private async Task ProcessNextAsync()
    {
        var processed = _orderBook.ProcessNextOrder(_inventory);
        if (processed is null) return;

        // === Robotdel: 3 hop fra a,b,c -> S ===
        await PickToS("a", 101);
        await PickToS("b", 102);
        await PickToS("c", 103);

        // conveyor flytter S automatisk (krav i opgaven)
        Console.WriteLine("Shipment box moved by conveyor belt.");

        // GUI-opdatering som før
        if (QueuedOrders.Count > 0) QueuedOrders.RemoveAt(0);
        ProcessedOrders.Add(processed);
        TotalRevenue = _orderBook.TotalRevenue();
    }

    // INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Async ICommand helper
public sealed class RelayCommandAsync : ICommand
{
    private readonly Func<object?, Task> _executeAsync;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    public RelayCommandAsync(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
        => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _executeAsync(parameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
