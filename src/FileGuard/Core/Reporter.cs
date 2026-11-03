using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileGuard.Core;

public static class Reporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task WriteAsync(TextWriter output, IReadOnlyList<Change> changes, bool json)
    {
        if (json)
        {
            var payload = new
            {
                checkedAtUtc = DateTime.UtcNow,
                totalChanges = changes.Count,
                changes
            };
            await output.WriteLineAsync(JsonSerializer.Serialize(payload, JsonOptions));
            return;
        }

        if (changes.Count == 0)
        {
            await output.WriteLineAsync("OK: no changes detected.");
            return;
        }

        await output.WriteLineAsync($"{changes.Count} change(s) detected:");
        foreach (var change in changes)
        {
            var tag = change.Type switch
            {
                ChangeType.Added => "[+] added   ",
                ChangeType.Modified => "[~] modified",
                ChangeType.Deleted => "[-] deleted ",
                _ => "[?]         "
            };
            await output.WriteLineAsync($"  {tag} {change.Path}");
        }
    }
}
