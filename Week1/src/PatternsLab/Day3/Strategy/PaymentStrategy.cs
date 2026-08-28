namespace PatternsLab.Day3.Strategy;

/// <summary>
/// Task 1.7 - Strategy pattern: ShoppingCart delegates the "how do we pay"
/// decision to a swappable IPaymentStrategy, so behaviour changes at runtime
/// without ShoppingCart itself changing.
/// </summary>
public interface IPaymentStrategy
{
    string Pay(decimal amount);
}

public class CreditCardPayment : IPaymentStrategy
{
    private readonly string _cardNumberMasked;
    public CreditCardPayment(string cardNumberMasked) => _cardNumberMasked = cardNumberMasked;

    public string Pay(decimal amount) =>
        $"Charged {amount:C} to credit card {_cardNumberMasked}.";
}

public class UpiPayment : IPaymentStrategy
{
    private readonly string _upiId;
    public UpiPayment(string upiId) => _upiId = upiId;

    public string Pay(decimal amount) =>
        $"Debited {amount:C} via UPI id {_upiId}.";
}

public class NetBankingPayment : IPaymentStrategy
{
    private readonly string _bankName;
    public NetBankingPayment(string bankName) => _bankName = bankName;

    public string Pay(decimal amount) =>
        $"Transferred {amount:C} via {_bankName} net banking.";
}

public class ShoppingCart
{
    private IPaymentStrategy _strategy;
    private decimal _total;

    public ShoppingCart(IPaymentStrategy initialStrategy)
    {
        _strategy = initialStrategy;
    }

    public void AddItem(decimal price) => _total += price;

    public void SetPaymentStrategy(IPaymentStrategy strategy) => _strategy = strategy;

    public string Checkout()
    {
        string receipt = _strategy.Pay(_total);
        _total = 0m;
        return receipt;
    }
}

public static class PaymentStrategyDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.7: Strategy (payments) ---");

        var cart = new ShoppingCart(new CreditCardPayment("**** **** **** 4242"));
        cart.AddItem(999m);
        cart.AddItem(250m);
        Console.WriteLine(cart.Checkout());

        cart.AddItem(1200m);
        cart.SetPaymentStrategy(new UpiPayment("asha@upi"));
        Console.WriteLine(cart.Checkout());

        cart.AddItem(500m);
        cart.SetPaymentStrategy(new NetBankingPayment("HDFC Bank"));
        Console.WriteLine(cart.Checkout());
    }
}
