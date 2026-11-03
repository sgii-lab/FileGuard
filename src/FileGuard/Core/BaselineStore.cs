using System.Text.Json;

namespace FileGuard.Core;

public static class BaselineStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static async Task SaveAsync(string path, Baseline baseline, CancellationToken ct = default)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, baseline, Options, ct);
    }

    public static async Task<Baseline> LoadAsync(string path, CancellationToken ct = default)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Baseline not found: {path}. Run 'fileguard init <path>' first.", path);

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Baseline>(stream, Options, ct)
               ?? throw new InvalidDataException("Baseline file is empty or invalid.");
    }
}
