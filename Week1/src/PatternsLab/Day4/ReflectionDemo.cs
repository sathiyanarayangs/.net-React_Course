using System.Reflection;

namespace PatternsLab.Day4;

/// <summary>
/// Task 1.11 - Reflect over an Invoice type: print its shape, then build
/// and populate an instance purely through reflection (Activator +
/// PropertyInfo.SetValue), without ever writing `new Invoice()`.
/// </summary>
public class Invoice
{
    public int InvoiceNumber { get; set; }
    public string Customer { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedOn { get; set; }

    public Invoice() { }

    public Invoice(int invoiceNumber, string customer)
    {
        InvoiceNumber = invoiceNumber;
        Customer = customer;
    }

    public decimal ApplyDiscount(decimal percent) => Amount - (Amount * percent / 100m);

    public override string ToString() =>
        $"Invoice #{InvoiceNumber} for {Customer}: {Amount:C} (issued {IssuedOn:d})";
}

public static class ReflectionDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.11: Reflection over Invoice ---");

        Type type = typeof(Invoice);
        Console.WriteLine($"Class name: {type.Name}");

        Console.WriteLine("Properties:");
        foreach (PropertyInfo prop in type.GetProperties())
        {
            Console.WriteLine($"  {prop.PropertyType.Name} {prop.Name}");
        }

        Console.WriteLine("Methods (declared on Invoice only):");
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Console.WriteLine($"  {method.Name}");
        }

        Console.WriteLine("Constructors:");
        foreach (ConstructorInfo ctor in type.GetConstructors())
        {
            var paramList = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"  Invoice({paramList})");
        }

        // Create an instance and set properties entirely via reflection.
        object? instance = Activator.CreateInstance(type);
        if (instance is null)
            throw new InvalidOperationException("Activator.CreateInstance returned null.");

        SetProperty(type, instance, nameof(Invoice.InvoiceNumber), 1042);
        SetProperty(type, instance, nameof(Invoice.Customer), "Nova Traders");
        SetProperty(type, instance, nameof(Invoice.Amount), 15499.99m);
        SetProperty(type, instance, nameof(Invoice.IssuedOn), DateTime.Today);

        Console.WriteLine($"Built via reflection: {instance}");
    }

    private static void SetProperty(Type type, object instance, string propertyName, object value)
    {
        PropertyInfo? prop = type.GetProperty(propertyName);
        prop?.SetValue(instance, value);
    }
}
