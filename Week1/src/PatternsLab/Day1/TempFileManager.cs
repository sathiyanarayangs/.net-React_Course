namespace PatternsLab.Day1;

/// <summary>
/// Task 1.2 - Creates a temp file in the constructor and deletes it on Dispose.
/// Implements the full Dispose pattern: a finalizer as a safety net for callers
/// who forget to dispose, plus GC.SuppressFinalize so that when Dispose() *is*
/// called deterministically, the GC doesn't pay to run the finalizer too.
/// </summary>
public sealed class TempFileManager : IDisposable
{
    public string FilePath { get; }
    private bool _disposed;

    public TempFileManager()
    {
        FilePath = Path.Combine(Path.GetTempPath(), $"patternslab_{Guid.NewGuid():N}.tmp");
        File.WriteAllText(FilePath, "temporary scratch data");
        Console.WriteLine($"[TempFileManager] Created: {FilePath}");
    }

    // Finalizer: last-resort cleanup if Dispose() was never called (e.g. the
    // caller forgot the `using`). Only unmanaged-ish resources (here, the file
    // on disk) should be touched from here.
    ~TempFileManager()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        // Tell the GC this object no longer needs finalizing: we already cleaned
        // up deterministically, so running the finalizer later would be wasted
        // work (and, if it touched managed objects, unsafe).
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Console.WriteLine($"[TempFileManager] Deleted: {FilePath}");
        }

        // `disposing` distinguishes a deterministic Dispose() call (where it's
        // safe to also release other managed IDisposable members) from a
        // finalizer call (where only unmanaged cleanup should happen).
        _disposed = true;
    }
}

public static class TempFileManagerDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.2: TempFileManager ---");
        string capturedPath;

        using (var manager = new TempFileManager())
        {
            capturedPath = manager.FilePath;
            Console.WriteLine($"Exists inside using block? {File.Exists(capturedPath)}");
        } // Dispose() runs here automatically

        Console.WriteLine($"Exists after using block?  {File.Exists(capturedPath)}");
    }
}
