using System.Diagnostics;

namespace PatternsLab.Day4;

/// <summary>
/// Task 1.10 - Time a sequential foreach vs Thread-based, Task.Run-based, and
/// Parallel.ForEach over 100 simulated 100ms operations, to show
/// Parallel.ForEach winning by exploiting multiple cores.
/// </summary>
public static class TplDemo
{
    private const int OperationCount = 100;
    private const int OperationDelayMs = 100;

    private static void SimulatedWork(int _) => Thread.Sleep(OperationDelayMs);

    public static void Run()
    {
        Console.WriteLine("--- Task 1.10: Thread vs Task.Run vs Parallel.ForEach ---");

        var items = Enumerable.Range(0, OperationCount).ToArray();

        long sequentialMs = Time(() =>
        {
            foreach (var item in items)
                SimulatedWork(item);
        });
        Console.WriteLine($"Sequential foreach:   {sequentialMs} ms");

        long threadMs = Time(() =>
        {
            // Chunk the work across a handful of raw Threads.
            int workerCount = Environment.ProcessorCount;
            var chunks = Chunk(items, workerCount);
            var threads = chunks.Select(chunk => new Thread(() =>
            {
                foreach (var item in chunk) SimulatedWork(item);
            })).ToList();
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();
        });
        Console.WriteLine($"Manual Thread pool:   {threadMs} ms");

        long taskRunMs = Time(() =>
        {
            var tasks = items.Select(item => Task.Run(() => SimulatedWork(item))).ToArray();
            Task.WaitAll(tasks);
        });
        Console.WriteLine($"Task.Run per item:    {taskRunMs} ms");

        long parallelMs = Time(() =>
        {
            Parallel.ForEach(items, SimulatedWork);
        });
        Console.WriteLine($"Parallel.ForEach:     {parallelMs} ms");

        Console.WriteLine($"Parallel.ForEach faster than sequential? {parallelMs < sequentialMs}");
    }

    private static long Time(Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    private static IEnumerable<int[]> Chunk(int[] items, int chunkCount)
    {
        int size = (int)Math.Ceiling(items.Length / (double)chunkCount);
        for (int i = 0; i < items.Length; i += size)
            yield return items.Skip(i).Take(size).ToArray();
    }
}
