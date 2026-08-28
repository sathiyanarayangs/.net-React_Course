namespace PatternsLab.Day5;

/// <summary>
/// Task 1.13 (part 2) - Static vs Instance.
///
/// MathHelper's operations (Factorial, IsPrime, GCD) are pure functions: given
/// the same input they always return the same output and need no per-object
/// state, so they're static — callable as MathHelper.Factorial(5) with no
/// instance required.
///
/// OrderProcessor, by contrast, needs to accumulate state across calls (the
/// running list of line items and totals for *this particular* order), so its
/// methods are instance methods on an object that carries that state.
/// </summary>
public static class MathHelper
{
    public static long Factorial(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Factorial is undefined for negative numbers.");
        long result = 1;
        for (int i = 2; i <= n; i++) result *= i;
        return result;
    }

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; (long)i * i <= n; i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

    public static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a;
    }
}

public class OrderProcessor
{
    private readonly List<(string Item, decimal Price)> _lines = new();

    // Instance state: which order this processor is working on.
    public string OrderId { get; }

    public OrderProcessor(string orderId) => OrderId = orderId;

    public void AddLine(string item, decimal price) => _lines.Add((item, price));

    public decimal Total() => _lines.Sum(l => l.Price);

    public string Summarize() =>
        $"Order {OrderId}: {_lines.Count} item(s), total {Total():C}";
}

public static class StaticVsInstanceDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.13: Static (MathHelper) vs Instance (OrderProcessor) ---");

        Console.WriteLine($"5! = {MathHelper.Factorial(5)}");
        Console.WriteLine($"IsPrime(17) = {MathHelper.IsPrime(17)}");
        Console.WriteLine($"GCD(48, 18) = {MathHelper.Gcd(48, 18)}");

        var order1 = new OrderProcessor("ORD-1");
        order1.AddLine("Keyboard", 2500m);
        order1.AddLine("Mouse", 800m);

        var order2 = new OrderProcessor("ORD-2");
        order2.AddLine("Monitor", 12000m);

        Console.WriteLine(order1.Summarize()); // each instance keeps its own state
        Console.WriteLine(order2.Summarize());
    }
}
