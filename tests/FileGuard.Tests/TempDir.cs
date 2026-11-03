namespace FileGuard.Tests;

/// <summary>A throwaway directory that deletes itself when disposed.</summary>
internal sealed class TempDir : IDisposable
{
    public string Root { get; } =
        Path.Combine(Path.GetTempPath(), "fileguard-tests-" + Guid.NewGuid().ToString("N"));

    public TempDir() => Directory.CreateDirectory(Root);

    public string Write(string relativePath, string content)
    {
        var full = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
        return full;
    }

    public void Dispose()
    {
        try { Directory.Delete(Root, recursive: true); }
        catch { /* best effort cleanup */ }
    }
}
