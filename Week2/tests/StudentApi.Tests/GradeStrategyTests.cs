using StudentApi.Patterns;
using Xunit;

namespace StudentApi.Tests;

public class GradeStrategyTests
{
    [Theory]
    [InlineData(95, "A")]
    [InlineData(85, "B")]
    [InlineData(75, "C")]
    [InlineData(65, "D")]
    [InlineData(40, "F")]
    public void PercentageGradeStrategy_ReturnsExpectedLetter(decimal score, string expected)
    {
        var strategy = new PercentageGradeStrategy();
        Assert.Equal(expected, strategy.CalculateGrade(score));
    }

    [Theory]
    [InlineData(100, "4.00")]
    [InlineData(50, "2.00")]
    [InlineData(0, "0.00")]
    public void GpaGradeStrategy_ReturnsExpectedGpa(decimal score, string expected)
    {
        var strategy = new GpaGradeStrategy();
        Assert.Equal(expected, strategy.CalculateGrade(score));
    }

    private static IGradeStrategyFactory CreateFactory() =>
        new GradeStrategyFactory(new IGradeStrategy[] { new PercentageGradeStrategy(), new GpaGradeStrategy() });

    [Fact]
    public void Factory_ReturnsPercentageStrategy_ByDefault_WhenKeyIsNull()
    {
        var factory = CreateFactory();
        var strategy = factory.GetStrategy(null);
        Assert.IsType<PercentageGradeStrategy>(strategy);
    }

    [Fact]
    public void Factory_ReturnsPercentageStrategy_ByDefault_WhenKeyIsEmpty()
    {
        var factory = CreateFactory();
        var strategy = factory.GetStrategy("   ");
        Assert.IsType<PercentageGradeStrategy>(strategy);
    }

    [Theory]
    [InlineData("percentage", typeof(PercentageGradeStrategy))]
    [InlineData("PERCENTAGE", typeof(PercentageGradeStrategy))]
    [InlineData("gpa", typeof(GpaGradeStrategy))]
    [InlineData("GPA", typeof(GpaGradeStrategy))]
    public void Factory_SelectsCorrectStrategy_CaseInsensitively(string key, Type expectedType)
    {
        var factory = CreateFactory();
        var strategy = factory.GetStrategy(key);
        Assert.IsType(expectedType, strategy);
    }

    [Fact]
    public void Factory_ThrowsForUnknownKey()
    {
        var factory = CreateFactory();
        Assert.Throws<ArgumentException>(() => factory.GetStrategy("letter-grade-plus"));
    }
}
