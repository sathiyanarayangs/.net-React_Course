namespace PatternsLab.Day2;

/// <summary>
/// Task 1.4 - Thread-safe Singleton via Lazy&lt;T&gt;. Lazy&lt;T&gt; defaults to
/// LazyThreadSafetyMode.ExecutionAndPublication, which guarantees only one
/// instance is ever constructed even under concurrent first access.
/// </summary>
public sealed class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());

    public static Logger Instance => _instance.Value;

    private readonly object _writeLock = new();
    private readonly List<string> _entries = new();

    private Logger()
    {
        // Private constructor: the only way to get a Logger is Logger.Instance.
    }

    public void Log(string message)
    {
        lock (_writeLock)
        {
            _entries.Add(message);
        }
    }

    public int EntryCount
    {
        get { lock (_writeLock) { return _entries.Count; } }
    }
}

public static class LoggerDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.4: Thread-safe Singleton Logger ---");

        var hashCodes = new System.Collections.Concurrent.ConcurrentBag<int>();

        // 5 raw Threads
        var threads = new List<Thread>();
        for (int i = 0; i < 5; i++)
        {
            int id = i;
            var t = new Thread(() =>
            {
                var logger = Logger.Instance;
                logger.Log($"thread-{id} says hello");
                hashCodes.Add(logger.GetHashCode());
            });
            threads.Add(t);
            t.Start();
        }
        foreach (var t in threads) t.Join();

        // 5 Tasks
        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            int id = i;
            tasks.Add(Task.Run(() =>
            {
                var logger = Logger.Instance;
                logger.Log($"task-{id} says hello");
                hashCodes.Add(logger.GetHashCode());
            }));
        }
        Task.WaitAll(tasks.ToArray());

        foreach (var hash in hashCodes)
        {
            Console.WriteLine($"Instance hash code seen: {hash}");
        }

        bool allSame = hashCodes.Distinct().Count() == 1;
        Console.WriteLine($"All calls shared one instance? {allSame} " +
                           $"(entries logged: {Logger.Instance.EntryCount})");
    }
}
