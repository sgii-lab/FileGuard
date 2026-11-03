using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace FileGuard.Core;

/// <summary>Walks a directory tree and computes a SHA-256 hash of every file, in parallel.</summary>
public class Scanner
{
    private readonly List<string> _excludes;

    public Scanner(IEnumerable<string>? excludes = null)
    {
        _excludes = excludes?.ToList() ?? new List<string>();
    }

    public async Task<ScanResult> ScanAsync(string root, CancellationToken ct = default)
    {
        var rootFull = Path.GetFullPath(root);
        if (!Directory.Exists(rootFull))
            throw new DirectoryNotFoundException($"Directory not found: {rootFull}");

        var enumeration = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            // Skip symlinks so we never follow a link out of the tree or loop forever.
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        var candidates = Directory.EnumerateFiles(rootFull, "*", enumeration)
            .Select(full => (Full: full, Relative: Path.GetRelativePath(rootFull, full).Replace('\\', '/')))
            .Where(f => !_excludes.Any(pattern => Glob.IsMatch(pattern, f.Relative)))
            .ToList();

        var entries = new ConcurrentBag<FileEntry>();
        var skipped = new ConcurrentBag<string>();

        var parallel = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(candidates, parallel, async (file, token) =>
        {
            try
            {
                await using var stream = new FileStream(
                    file.Full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                    bufferSize: 81920, useAsync: true);

                var hash = await SHA256.HashDataAsync(stream, token);
                entries.Add(new FileEntry(
                    file.Relative,
                    Convert.ToHexString(hash).ToLowerInvariant(),
                    stream.Length));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                skipped.Add($"{file.Relative} ({ex.Message})");
            }
        });

        return new ScanResult(
            entries.OrderBy(e => e.Path, StringComparer.Ordinal).ToList(),
            skipped.OrderBy(s => s, StringComparer.Ordinal).ToList());
    }
}
