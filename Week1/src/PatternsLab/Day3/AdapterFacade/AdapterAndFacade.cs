using System.Text;
using System.Text.Json;

namespace PatternsLab.Day3.AdapterFacade;

// ---------------------------------------------------------------------
// Adapter: a third-party report generator only accepts XML, but our
// system produces JSON. XmlReportAdapter converts JSON -> the XML shape
// the legacy generator expects, without changing either side.
// ---------------------------------------------------------------------

/// <summary>Stand-in for a third-party library that only understands XML.</summary>
public class ThirdPartyXmlReportGenerator
{
    public string Generate(string xml)
    {
        return $"[ThirdPartyXmlReportGenerator] rendered report from:\n{xml}";
    }
}

public interface IReportGenerator
{
    string GenerateFromJson(string json);
}

public class XmlReportAdapter : IReportGenerator
{
    private readonly ThirdPartyXmlReportGenerator _legacyGenerator;

    public XmlReportAdapter(ThirdPartyXmlReportGenerator legacyGenerator)
    {
        _legacyGenerator = legacyGenerator;
    }

    public string GenerateFromJson(string json)
    {
        string xml = ConvertJsonToXml(json);
        return _legacyGenerator.Generate(xml);
    }

    private static string ConvertJsonToXml(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var sb = new StringBuilder();
        sb.Append("<report>");
        AppendElement(sb, doc.RootElement);
        sb.Append("</report>");
        return sb.ToString();
    }

    private static void AppendElement(StringBuilder sb, JsonElement element)
    {
        foreach (var property in element.EnumerateObject())
        {
            sb.Append($"<{property.Name}>{property.Value}</{property.Name}>");
        }
    }
}

// ---------------------------------------------------------------------
// Facade: OrderFacade hides the coordination of three subsystems
// (Inventory, Payment, Shipping) behind one simple call.
// ---------------------------------------------------------------------

public class InventorySubsystem
{
    public bool Reserve(string sku, int quantity)
    {
        Console.WriteLine($"[Inventory] Reserved {quantity}x {sku}.");
        return true;
    }
}

public class PaymentSubsystem
{
    public bool Charge(decimal amount)
    {
        Console.WriteLine($"[Payment] Charged {amount:C}.");
        return true;
    }
}

public class ShippingSubsystem
{
    public string Ship(string sku, string address)
    {
        Console.WriteLine($"[Shipping] Shipped {sku} to {address}.");
        return $"TRACK-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }
}

public class OrderFacade
{
    private readonly InventorySubsystem _inventory = new();
    private readonly PaymentSubsystem _payment = new();
    private readonly ShippingSubsystem _shipping = new();

    public string PlaceOrder(string sku, int quantity, decimal amount, string address)
    {
        if (!_inventory.Reserve(sku, quantity))
            throw new InvalidOperationException("Out of stock.");

        if (!_payment.Charge(amount))
            throw new InvalidOperationException("Payment declined.");

        string trackingNumber = _shipping.Ship(sku, address);
        return trackingNumber;
    }
}

public static class AdapterFacadeDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.9: Adapter (JSON->XML) + Facade (OrderFacade) ---");

        var adapter = new XmlReportAdapter(new ThirdPartyXmlReportGenerator());
        string json = """{"title":"Q3 Sales","total":184500}""";
        Console.WriteLine(adapter.GenerateFromJson(json));

        var orderFacade = new OrderFacade();
        string tracking = orderFacade.PlaceOrder("SKU-1001", quantity: 2, amount: 2499m, address: "12 MG Road, Chennai");
        Console.WriteLine($"Order placed. Tracking number: {tracking}");
    }
}
