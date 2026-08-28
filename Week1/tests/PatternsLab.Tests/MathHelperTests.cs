using PatternsLab.Day5;
using Xunit;

namespace PatternsLab.Tests;

public class MathHelperTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(10, 3628800)]
    public void Factorial_ReturnsExpectedValue(int n, long expected)
    {
        Assert.Equal(expected, MathHelper.Factorial(n));
    }

    [Fact]
    public void Factorial_ThrowsForNegativeInput()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathHelper.Factorial(-1));
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(17, true)]
    [InlineData(97, true)]
    [InlineData(4, false)]
    [InlineData(1, false)]
    [InlineData(0, false)]
    [InlineData(-7, false)]
    public void IsPrime_ReturnsExpectedResult(int n, bool expected)
    {
        Assert.Equal(expected, MathHelper.IsPrime(n));
    }

    [Theory]
    [InlineData(48, 18, 6)]
    [InlineData(0, 5, 5)]
    [InlineData(5, 0, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(-12, 18, 6)]
    [InlineData(7, 13, 1)]
    public void Gcd_ReturnsExpectedResult(int a, int b, int expected)
    {
        Assert.Equal(expected, MathHelper.Gcd(a, b));
    }
}
