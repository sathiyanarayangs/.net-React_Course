using System.Reflection;

namespace PatternsLab.Day4.Attributes;

/// <summary>
/// Task 1.12 - Custom attribute declaring a max string length, plus a
/// reflection-based validator that scans an object's string properties and
/// warns for any value exceeding its declared limit.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class MaxLengthAttribute : Attribute
{
    public int MaxLength { get; }
    public MaxLengthAttribute(int maxLength) => MaxLength = maxLength;
}

public class User
{
    [MaxLength(10)]
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty; // no attribute -> not checked
}

public static class AttributeValidator
{
    /// <summary>Returns one warning string per property that exceeds its [MaxLength(n)].</summary>
    public static List<string> Validate(object obj)
    {
        var warnings = new List<string>();
        Type type = obj.GetType();

        foreach (PropertyInfo prop in type.GetProperties())
        {
            var attr = prop.GetCustomAttribute<MaxLengthAttribute>();
            if (attr is null) continue;

            if (prop.GetValue(obj) is string value && value.Length > attr.MaxLength)
            {
                warnings.Add(
                    $"{type.Name}.{prop.Name} is {value.Length} chars, exceeds MaxLength({attr.MaxLength}): \"{value}\"");
            }
        }

        return warnings;
    }
}

public static class AttributeValidatorDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.12: Custom [MaxLength] attribute + validator ---");

        var okUser = new User { Name = "Asha", Email = "asha@example.com" };
        var badUser = new User { Name = "A Very Long Username Indeed", Email = "x@example.com" };

        foreach (var user in new[] { okUser, badUser })
        {
            var warnings = AttributeValidator.Validate(user);
            if (warnings.Count == 0)
            {
                Console.WriteLine($"'{user.Name}': OK, no warnings.");
            }
            else
            {
                foreach (var w in warnings)
                    Console.WriteLine($"WARNING: {w}");
            }
        }
    }
}
