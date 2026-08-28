using System.Collections.Concurrent;
using PatternsLab.Day2;
using Xunit;

namespace PatternsLab.Tests;

public class LoggerSingletonTests
{
    [Fact]
    public void Instance_ReturnsSameObject_OnRepeatedCalls()
    {
        var first = Logger.Instance;
        var second = Logger.Instance;

        Assert.Same(first, second);
    }

    [Fact]
    public void Instance_IsSameAcrossConcurrentThreads()
    {
        var hashCodes = new ConcurrentBag<int>();

        var threads = Enumerable.Range(0, 8).Select(_ => new Thread(() =>
        {
            hashCodes.Add(Logger.Instance.GetHashCode());
        })).ToList();

        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();

        Assert.Single(hashCodes.Distinct());
    }

    [Fact]
    public void Log_IncreasesEntryCount()
    {
        int before = Logger.Instance.EntryCount;
        Logger.Instance.Log("test entry");
        int after = Logger.Instance.EntryCount;

        Assert.Equal(before + 1, after);
    }
}
