namespace SecureFileVault.Core.FileIO;

/// <summary>
/// Task 3.1 - FileStream write/read/append/copy, all in fixed-size chunks.
/// No File.ReadAllBytes anywhere: reading a large file whole into a byte[]
/// defeats the entire point of streaming (it's what Task 3.12 exists to fix),
/// so this class sets the habit from Day 1 onward.
/// </summary>
public static class ChunkedFileCopier
{
    public const int DefaultChunkSize = 4096; // 4 KB, per the task

    public static void WriteAllText(string path, string content)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    } // both `using`s dispose (and flush) here, in reverse declaration order

    public static string ReadAllTextChunked(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        var buffer = new char[DefaultChunkSize];
        var sb = new System.Text.StringBuilder();

        int charsRead;
        while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            sb.Append(buffer, 0, charsRead);
        }

        return sb.ToString();
    }

    public static void Append(string path, string content)
    {
        using var stream = new FileStream(path, FileMode.Append, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }

    /// <summary>
    /// Copies source to destination in fixed-size chunks via raw byte reads,
    /// never materializing the whole file in memory at once.
    /// </summary>
    public static void CopyChunked(string sourcePath, string destinationPath, int chunkSize = DefaultChunkSize)
    {
        using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);

        var buffer = new byte[chunkSize];
        int bytesRead;
        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            destination.Write(buffer, 0, bytesRead);
        }
    }

    /// <summary>Byte-for-byte comparison, also done in chunks rather than loading both files whole.</summary>
    public static bool FilesAreByteIdentical(string pathA, string pathB, int chunkSize = DefaultChunkSize)
    {
        using var a = new FileStream(pathA, FileMode.Open, FileAccess.Read);
        using var b = new FileStream(pathB, FileMode.Open, FileAccess.Read);

        if (a.Length != b.Length) return false;

        var bufferA = new byte[chunkSize];
        var bufferB = new byte[chunkSize];

        int readA;
        while ((readA = a.Read(bufferA, 0, chunkSize)) > 0)
        {
            int readB = b.Read(bufferB, 0, chunkSize);
            if (readA != readB) return false;
            if (!bufferA.AsSpan(0, readA).SequenceEqual(bufferB.AsSpan(0, readB))) return false;
        }

        return true;
    }
}
