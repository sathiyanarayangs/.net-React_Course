namespace PatternsLab.Day5;

/// <summary>
/// Task 1.14 - Employee implements IComparable&lt;Employee&gt; for its "natural"
/// ordering (by salary). EmployeeNameComparer implements IComparer&lt;Employee&gt;
/// as an alternative ordering (by name) without touching Employee itself.
/// </summary>
public class Employee : IComparable<Employee>
{
    public string Name { get; }
    public decimal Salary { get; }

    public Employee(string name, decimal salary)
    {
        Name = name;
        Salary = salary;
    }

    public int CompareTo(Employee? other)
    {
        if (other is null) return 1;
        return Salary.CompareTo(other.Salary);
    }

    public override string ToString() => $"{Name} ({Salary:C})";
}

public class EmployeeNameComparer : IComparer<Employee>
{
    public int Compare(Employee? x, Employee? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}

public static class SortingDemo
{
    public static List<Employee> BuildSampleEmployees() => new()
    {
        new Employee("Rahul", 65000m),
        new Employee("Priya", 82000m),
        new Employee("Zoya", 48000m),
        new Employee("Manoj", 91000m),
        new Employee("Anjali", 55000m),
        new Employee("Karthik", 73000m),
        new Employee("Divya", 60000m),
        new Employee("Ibrahim", 77000m),
        new Employee("Neha", 68000m),
        new Employee("Suresh", 52000m),
    };

    public static void Run()
    {
        Console.WriteLine("--- Task 1.14: IComparable/IComparer sorting ---");

        var bySalary = BuildSampleEmployees();
        bySalary.Sort(); // uses Employee.CompareTo -> salary order
        Console.WriteLine("Sorted by salary (default Sort()):");
        foreach (var e in bySalary) Console.WriteLine($"  {e}");

        var byName = BuildSampleEmployees();
        byName.Sort(new EmployeeNameComparer());
        Console.WriteLine("Sorted by name (EmployeeNameComparer):");
        foreach (var e in byName) Console.WriteLine($"  {e}");
    }
}
