using PatternsLab.Day3.Strategy;
using Xunit;

namespace PatternsLab.Tests;

public class PaymentStrategyTests
{
    [Fact]
    public void CreditCardPayment_ReturnsReceiptMentioningAmount()
    {
        var strategy = new CreditCardPayment("**** 4242");
        string receipt = strategy.Pay(1500m);
        Assert.Contains("1,500.00", receipt);
        Assert.Contains("credit card", receipt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UpiPayment_ReturnsReceiptMentioningUpiId()
    {
        var strategy = new UpiPayment("asha@upi");
        string receipt = strategy.Pay(250m);
        Assert.Contains("asha@upi", receipt);
    }

    [Fact]
    public void NetBankingPayment_ReturnsReceiptMentioningBank()
    {
        var strategy = new NetBankingPayment("HDFC Bank");
        string receipt = strategy.Pay(999m);
        Assert.Contains("HDFC Bank", receipt);
    }

    [Fact]
    public void ShoppingCart_UsesInitialStrategy_WhenNoSwap()
    {
        var cart = new ShoppingCart(new CreditCardPayment("**** 1111"));
        cart.AddItem(100m);
        string receipt = cart.Checkout();
        Assert.Contains("credit card", receipt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShoppingCart_SwappingStrategyAtRuntime_ChangesCheckoutBehaviour()
    {
        var cart = new ShoppingCart(new CreditCardPayment("**** 1111"));
        cart.AddItem(500m);
        cart.SetPaymentStrategy(new UpiPayment("test@upi"));

        string receipt = cart.Checkout();

        Assert.Contains("UPI", receipt);
        Assert.DoesNotContain("credit card", receipt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShoppingCart_Checkout_ResetsTotalToZero()
    {
        var cart = new ShoppingCart(new CreditCardPayment("**** 1111"));
        cart.AddItem(500m);
        cart.Checkout();

        cart.AddItem(10m);
        string secondReceipt = cart.Checkout();

        Assert.Contains("10.00", secondReceipt);
        Assert.DoesNotContain("510", secondReceipt);
    }
}
