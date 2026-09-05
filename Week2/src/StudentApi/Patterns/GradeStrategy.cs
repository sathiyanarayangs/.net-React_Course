namespace StudentApi.Patterns;

/// <summary>
/// Task 2.8 - the applied API pattern. A student's numeric Score (0-100) can
/// be presented either as a letter grade based on percentage bands, or as a
/// 4.0-scale GPA. Which one is used is chosen per request (via a query
/// string / header, see StudentsController), and the controller never knows
/// or cares which strategy ran.
/// </summary>
public interface IGradeStrategy
{
    /// <summary>A short name used to select this strategy (e.g. "percentage", "gpa").</summary>
    string Key { get; }

    /// <summary>Converts a raw 0-100 score into this strategy's display grade.</summary>
    string CalculateGrade(decimal score);
}

public class PercentageGradeStrategy : IGradeStrategy
{
    public string Key => "percentage";

    public string CalculateGrade(decimal score) => score switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F"
    };
}

public class GpaGradeStrategy : IGradeStrategy
{
    public string Key => "gpa";

    public string CalculateGrade(decimal score)
    {
        // Simple linear mapping of a 0-100 score onto a 0.0-4.0 GPA scale.
        decimal gpa = Math.Round(score / 100m * 4.0m, 2);
        return gpa.ToString("F2");
    }
}

/// <summary>
/// Factory that selects the right IGradeStrategy by key. New strategies
/// (e.g. "pass-fail") only require adding one line here — the controller
/// and service never change.
/// </summary>
public interface IGradeStrategyFactory
{
    IGradeStrategy GetStrategy(string? key);
}

public class GradeStrategyFactory : IGradeStrategyFactory
{
    private readonly Dictionary<string, IGradeStrategy> _strategies;

    public GradeStrategyFactory(IEnumerable<IGradeStrategy> strategies)
    {
        // DI hands us every registered IGradeStrategy; index them by key.
        _strategies = strategies.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);
    }

    public IGradeStrategy GetStrategy(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) key = "percentage"; // default

        return _strategies.TryGetValue(key, out var strategy)
            ? strategy
            : throw new ArgumentException($"Unknown grade strategy '{key}'. Valid options: {string.Join(", ", _strategies.Keys)}");
    }
}
