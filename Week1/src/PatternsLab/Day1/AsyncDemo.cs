using System.Diagnostics;

namespace PatternsLab.Day1;

/// <summary>
/// Task 1.3 - A simulated async fetch, then a comparison of sequential awaits
/// vs Task.WhenAll to show the concurrency speedup.
/// </summary>
public static class AsyncDemo
{
    public static async Task<string> FetchUserDataAsync(int userId)
    {
        Console.WriteLine($"[fetch {userId}] starting at {Stopwatch.GetTimestamp()}");
        await Task.Delay(3000); // simulate network / I/O latency
        Console.WriteLine($"[fetch {userId}] finished");
        return $"UserData#{userId}";
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("--- Task 1.3: async/await + Task.WhenAll ---");

        var sw = Stopwatch.StartNew();
        Console.WriteLine("Sequential fetches:");
        var r1 = await FetchUserDataAsync(1);
        var r2 = await FetchUserDataAsync(2);
        var r3 = await FetchUserDataAsync(3);
        sw.Stop();
        Console.WriteLine($"Sequential total: {sw.ElapsedMilliseconds} ms -> [{r1}, {r2}, {r3}]");

        sw.Restart();
        Console.WriteLine("Concurrent fetches (Task.WhenAll):");
        Task<string>[] tasks =
        {
            FetchUserDataAsync(4),
            FetchUserDataAsync(5),
            FetchUserDataAsync(6)
        };
        string[] results = await Task.WhenAll(tasks);
        sw.Stop();
        Console.WriteLine($"Concurrent total: {sw.ElapsedMilliseconds} ms -> [{string.Join(", ", results)}]");

        Console.WriteLine("Task.WhenAll finished in roughly one delay's worth of time " +
                           "instead of three, because the three awaits overlap.");
    }
}
