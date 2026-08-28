namespace PatternsLab.Day2.Observer;

// ---------------------------------------------------------------------
// Version A: classic Observer with a custom interface.
// ---------------------------------------------------------------------

public interface IStockObserver
{
    void OnPriceChanged(string symbol, decimal newPrice);
}

public class Investor : IStockObserver
{
    public string Name { get; }
    public Investor(string name) => Name = name;

    public void OnPriceChanged(string symbol, decimal newPrice)
    {
        Console.WriteLine($"[custom-interface] {Name} notified: {symbol} is now {newPrice:C}");
    }
}

public class StockTickerClassic
{
    private readonly List<IStockObserver> _observers = new();

    public void Subscribe(IStockObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IStockObserver observer) => _observers.Remove(observer);

    public void SetPrice(string symbol, decimal newPrice)
    {
        foreach (var observer in _observers)
        {
            observer.OnPriceChanged(symbol, newPrice);
        }
    }
}

// ---------------------------------------------------------------------
// Version B: idiomatic C# using events/delegates.
// ---------------------------------------------------------------------

public class PriceChangedEventArgs : EventArgs
{
    public string Symbol { get; }
    public decimal NewPrice { get; }

    public PriceChangedEventArgs(string symbol, decimal newPrice)
    {
        Symbol = symbol;
        NewPrice = newPrice;
    }
}

public class StockTickerEvents
{
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    public void SetPrice(string symbol, decimal newPrice)
    {
        // ?.Invoke: no-op if nobody has subscribed yet.
        PriceChanged?.Invoke(this, new PriceChangedEventArgs(symbol, newPrice));
    }
}

public class InvestorEventSubscriber
{
    public string Name { get; }
    public InvestorEventSubscriber(string name) => Name = name;

    public void Handle(object? sender, PriceChangedEventArgs e)
    {
        Console.WriteLine($"[C#-event] {Name} notified: {e.Symbol} is now {e.NewPrice:C}");
    }
}

public static class StockTickerDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.6: Observer (custom interface vs C# events) ---");

        // Custom interface version
        var classicTicker = new StockTickerClassic();
        var investorA = new Investor("Investor A");
        var investorB = new Investor("Investor B");
        classicTicker.Subscribe(investorA);
        classicTicker.Subscribe(investorB);
        classicTicker.SetPrice("INFY", 1500.25m);

        // C# events version
        var eventTicker = new StockTickerEvents();
        var investorC = new InvestorEventSubscriber("Investor C");
        var investorD = new InvestorEventSubscriber("Investor D");
        eventTicker.PriceChanged += investorC.Handle;
        eventTicker.PriceChanged += investorD.Handle;
        eventTicker.SetPrice("TCS", 3800.50m);

        // Comparison:
        // - Custom IObserver interface: explicit contract, easy to see who
        //   implements "observer-ness", but needs Subscribe/Unsubscribe
        //   plumbing you write yourself, and only one notification "shape"
        //   per interface unless you add more methods.
        // - C# events: built-in multicast delegate machinery (+=/-=), null-safe
        //   invocation via ?.Invoke, and integrates with the rest of .NET
        //   (data binding, async event patterns). Slightly more "magic" and
        //   harder to enumerate current subscribers than a List<IStockObserver>.
    }
}
