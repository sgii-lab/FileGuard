namespace FileGuard.Core;

public static class BaselineComparer
{
    /// <summary>Returns what changed between two snapshots, sorted by path.</summary>
    public static List<Change> Compare(IEnumerable<FileEntry> baseline, IEnumerable<FileEntry> current)
    {
        var before = baseline.ToDictionary(f => f.Path, StringComparer.Ordinal);
        var after = current.ToDictionary(f => f.Path, StringComparer.Ordinal);
        var changes = new List<Change>();

        foreach (var (path, entry) in after)
        {
            if (!before.TryGetValue(path, out var old))
                changes.Add(new Change(ChangeType.Added, path));
            else if (!string.Equals(old.Hash, entry.Hash, StringComparison.Ordinal))
                changes.Add(new Change(ChangeType.Modified, path));
        }

        foreach (var path in before.Keys)
        {
            if (!after.ContainsKey(path))
                changes.Add(new Change(ChangeType.Deleted, path));
        }

        return changes.OrderBy(c => c.Path, StringComparer.Ordinal).ToList();
    }
}
