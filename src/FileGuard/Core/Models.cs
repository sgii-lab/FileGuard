using System.Text.Json.Serialization;

namespace FileGuard.Core;

/// <summary>One file as recorded in a baseline. Path is relative to the scanned root, using '/'.</summary>
public record FileEntry(string Path, string Hash, long Size);

public class Baseline
{
    public int Version { get; set; } = 1;
    public DateTime CreatedUtc { get; set; }
    public string Root { get; set; } = string.Empty;
    public List<FileEntry> Files { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChangeType
{
    Added,
    Modified,
    Deleted
}

public record Change(ChangeType Type, string Path);

public record ScanResult(List<FileEntry> Files, List<string> Skipped);
