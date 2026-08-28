using PatternsLab.Day5;
using Xunit;

namespace PatternsLab.Tests;

public class SortingTests
{
    [Fact]
    public void Employee_CompareTo_OrdersBySalaryAscending()
    {
        var low = new Employee("Low", 30000m);
        var high = new Employee("High", 90000m);

        Assert.True(low.CompareTo(high) < 0);
        Assert.True(high.CompareTo(low) > 0);
    }

    [Fact]
    public void Employee_CompareTo_ReturnsPositive_WhenOtherIsNull()
    {
        var employee = new Employee("Someone", 50000m);
        Assert.True(employee.CompareTo(null) > 0);
    }

    [Fact]
    public void DefaultSort_OrdersEmployeesBySalary()
    {
        var employees = SortingDemo.BuildSampleEmployees();
        employees.Sort();

        var salaries = employees.Select(e => e.Salary).ToList();
        var expectedSorted = salaries.OrderBy(s => s).ToList();

        Assert.Equal(expectedSorted, salaries);
    }

    [Fact]
    public void EmployeeNameComparer_OrdersEmployeesByNameOrdinal()
    {
        var employees = SortingDemo.BuildSampleEmployees();
        employees.Sort(new EmployeeNameComparer());

        var names = employees.Select(e => e.Name).ToList();
        var expectedSorted = names.OrderBy(n => n, StringComparer.Ordinal).ToList();

        Assert.Equal(expectedSorted, names);
    }

    [Fact]
    public void EmployeeNameComparer_HandlesNullsWithoutThrowing()
    {
        var comparer = new EmployeeNameComparer();
        var employee = new Employee("Someone", 1000m);

        Assert.Equal(0, comparer.Compare(null, null));
        Assert.True(comparer.Compare(null, employee) < 0);
        Assert.True(comparer.Compare(employee, null) > 0);
    }
}
