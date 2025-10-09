using System;
using System.Collections.Generic;
using System.Linq;

namespace InventorySystem2.Models
{
    // ---
    // Denne fil modellerer kernen for et inventory system, jeg har valgt alt samlet i en fil for nemhedens skyld.
    // Jeg har brugt linq til forespørgsler, og korrekt datatyping som er for decimal for penge
    // Koden er designet, så den er let at binde i UI og til at test

    // ----- Base Item -----
    public class Item
    {
        // Name sat som string fordi det naturligt til visning/søgning
        public string Name { get; }

        // PricePerUnit er sat som decimal bruges til valuta for at undgå fejl
        public decimal PricePerUnit { get; }

        // Ctor sætter get-only properties => immutability-light. har enkapsuleret for mindre bivirkning
        public Item(string name, decimal pricePerUnit)
        {
            Name = name;
            PricePerUnit = pricePerUnit;
        }

        public override string ToString()
        {
            // "C" bruger systemets til valuta
            return $"{Name}: {PricePerUnit:C} per unit";
        }
        // Jeg har brugt overriding som er standard object metode
        // iHer holder vi det simpelt og accepterer reference lighed.
    }

    // ----- BulkItem som arver fra Item -----
    public class BulkItem : Item
    {
        // Enhed fx "kg", "L". string er her fleksibel, enum kunne stramme validering mere fx.
        public string MeasurementUnit { get; }

        public BulkItem(string name, decimal pricePerUnit, string measurementUnit)
            : base(name, pricePerUnit)
        {
            MeasurementUnit = measurementUnit;
        }

        public override string ToString()
        {
            return $"{Name}: {PricePerUnit:C} per {MeasurementUnit}";
        }
    }

    // ----- UnitItem (arver igen fra Item) -----
    public class UnitItem : Item
    {
        // double er passende til fysiske målinger (små afrundingsfejl tolereres).
        // her er det vægt, så double bruges
        public double Weight { get; }

        public UnitItem(string name, decimal pricePerUnit, double weight)
            : base(name, pricePerUnit)
        {
            Weight = weight;
        }

        public override string ToString()
        {
            return $"{Name}: {PricePerUnit:C} per unit ({Weight} kg each)";
        }
    }

    // ----- Inventory -----
    public class Inventory
    {
        // For stock er der Dictionary<Item,double> fordi vi har brug for opslag på Item
        // double mængde for styk og bulk
        public Dictionary<Item, double> Stock { get; }

        public Inventory(Dictionary<Item, double> stock)
        {
            Stock = stock;
        }

        public List<Item> LowStockItems()
        {
            // Brugte lingq udtryk Where, Select, ToList til at filtrere og projektere data.
            return Stock
                .Where(kv => kv.Value < 5)
                .Select(kv => kv.Key)
                .ToList();
        }
    }

    // ----- OrderLine -----
    public class OrderLine
    {
        // Min OrderLine refererer til et Item og en mængde.
        public Item Item { get; }
        public double Quantity { get; }

        public OrderLine(Item item, double quantity)
        {
            Item = item;
            Quantity = quantity;
        }
        
        // Cast til decimal før prisberegning for at bevare valuta-præcision.
        public decimal LineTotal => (decimal)Quantity * Item.PricePerUnit;

        public override string ToString()
        {
            return $"{Item.Name} x {Quantity} → {LineTotal:C}";
        }
    }

    // ----- Order -----
    public class Order
    {
        // tidspunkt for ordren er angivet som krævet i opgaven
        // datetimeoffset kan være mere robust ift. tidszoner men ja jeg kører det simpelt.
        public DateTime Time { get; }

        // En order består af flere OrderLines, det matcher behov for rækkefølge og iteration
        public List<OrderLine> OrderLines { get; }

        public Order(DateTime time, List<OrderLine> orderLines)
        {
            Time = time;
            OrderLines = orderLines;
        }

        // linq for Sum over projektion for at beregne aggregeret total.
        public decimal Total => OrderLines.Sum(l => l.LineTotal);

        // UI hjælp her kort og bindevenlig summary som DataGrid kan vise
        public string LinesSummary =>
            string.Join(", ", OrderLines.Select(l => $"{l.Item.Name} x {l.Quantity}"));

        public override string ToString()
        {
            return $"{Time:G} — {OrderLines.Count} lines, total {Total:C}";
        }
    }

    // ----- OrderBook -----
    public class OrderBook
    {
        // Vi modellerer et simpelt pipeline flow:
        // queuedorders, ordrer der venter
        // processedorders, historik til rapportering for total revenue
        public List<Order> QueuedOrders { get; } = new();
        public List<Order> ProcessedOrders { get; } = new();

        // Enkel mutation af samlinger med klar metodekontrakt.
        public void QueueOrder(Order o)
        {
            QueuedOrders.Add(o);
        }

        public Order? ProcessNextOrder(Inventory inv)
        {
            // Laver enGuard clause for noget struktur
            if (QueuedOrders.Count == 0) return null;

            var next = QueuedOrders[0];
            QueuedOrders.RemoveAt(0);        
            ProcessedOrders.Add(next);

            // Opgavekrav: opdater lager ved behandling.
            // vi tjekker nøglen findes, man kunne udvide med validering
            // mod negative lagertal og exceptions/logning ved manglende vare som et eksempel
            foreach (var line in next.OrderLines)
            {
                if (inv.Stock.ContainsKey(line.Item))
                    inv.Stock[line.Item] -= line.Quantity;
                // else: kunne håndteres eksplicit
            }

            return next;
        }

        // Rapportering fx for total omsætning fra behandlede ordrer.
        public decimal TotalRevenue() => ProcessedOrders.Sum(o => o.Total);
    }

    // ----- Customer -----
    public class Customer
    {
        // Simpelt kundeobjekt der ejer sine ordrer (komposition).
        public string Name { get; }
        public List<Order> Orders { get; } = new();

        public Customer(string name)
        {
            Name = name;
        }

        public void CreateOrder(Order o)
        {
            Orders.Add(o);
        }

        public override string ToString()
        {
            return $"{Name} ({Orders.Count} orders)";
        }
    }
}
