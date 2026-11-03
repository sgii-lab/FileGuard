using System.Text.Json;
using FileGuard.Core;

namespace FileGuard.Cli;

public static class App
{
    public const int ExitClean = 0;
    public const int ExitChangesFound = 1;
    public const int ExitError = 2;

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter? stdout = null,
        TextWriter? stderr = null,
        CancellationToken ct = default)
    {
        stdout ??= Console.Out;
        stderr ??= Console.Error;

        CliOptions options;
        try
        {
            options = ArgParser.Parse(args);
        }
        catch (ArgumentException ex)
        {
            await stderr.WriteLineAsync($"Error: {ex.Message}");
            await stderr.WriteLineAsync("Run 'fileguard --help' for usage.");
            return ExitError;
        }

        if (options.ShowHelp)
        {
            await stdout.WriteLineAsync(HelpText.Usage);
            return ExitClean;
        }

        if ((options.Command is "init" or "check" or "watch") && options.Path is null)
        {
            await stderr.WriteLineAsync($"Error: '{options.Command}' needs a directory path.");
            await stderr.WriteLineAsync("Run 'fileguard --help' for usage.");
            return ExitError;
        }

        try
        {
            switch (options.Command)
            {
                case "init":
                    return await InitAsync(options, stdout, stderr, ct);
                case "check":
                    return await CheckAsync(options, stdout, stderr, ct);
                case "watch":
                    return await WatchAsync(options, stdout, stderr, ct);
                default:
                    await stderr.WriteLineAsync($"Error: unknown command '{options.Command}'.");
                    await stderr.WriteLineAsync("Run 'fileguard --help' for usage.");
                    return ExitError;
            }
        }
        catch (OperationCanceledException)
        {
            await stdout.WriteLineAsync("Stopped.");
            return ExitClean;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or JsonException)
        {
            await stderr.WriteLineAsync($"Error: {ex.Message}");
            return ExitError;
        }
    }

    private static async Task<int> InitAsync(CliOptions options, TextWriter stdout, TextWriter stderr, CancellationToken ct)
    {
        var root = Path.GetFullPath(options.Path!);
        var result = await BuildScanner(options, root).ScanAsync(root, ct);
        await ReportSkippedAsync(result, stderr);

        var baseline = new Baseline
        {
            CreatedUtc = DateTime.UtcNow,
            Root = root,
            Files = result.Files
        };
        await BaselineStore.SaveAsync(options.BaselinePath, baseline, ct);

        await stdout.WriteLineAsync(
            $"Baseline created: {result.Files.Count} file(s) hashed -> {options.BaselinePath}");
        return ExitClean;
    }

    private static async Task<int> CheckAsync(CliOptions options, TextWriter stdout, TextWriter stderr, CancellationToken ct)
    {
        var root = Path.GetFullPath(options.Path!);
        var baseline = await BaselineStore.LoadAsync(options.BaselinePath, ct);
        var result = await BuildScanner(options, root).ScanAsync(root, ct);
        await ReportSkippedAsync(result, stderr);

        var changes = BaselineComparer.Compare(baseline.Files, result.Files);
        await Reporter.WriteAsync(stdout, changes, options.Json);

        return changes.Count == 0 ? ExitClean : ExitChangesFound;
    }

    private static async Task<int> WatchAsync(CliOptions options, TextWriter stdout, TextWriter stderr, CancellationToken ct)
    {
        var root = Path.GetFullPath(options.Path!);
        var baseline = await BaselineStore.LoadAsync(options.BaselinePath, ct);
        var scanner = BuildScanner(options, root);

        await stdout.WriteLineAsync(
            $"Watching {root} every {options.IntervalSeconds}s. Press Ctrl+C to stop.");

        // The first pass compares against the saved baseline; after that, each pass
        // reports only what changed since the previous pass.
        var previous = baseline.Files;
        var firstScan = true;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var result = await scanner.ScanAsync(root, ct);
            if (firstScan)
            {
                await ReportSkippedAsync(result, stderr);
                firstScan = false;
            }

            var changes = BaselineComparer.Compare(previous, result.Files);
            if (changes.Count > 0)
            {
                if (!options.Json)
                    await stdout.WriteLineAsync($"[{DateTime.Now:HH:mm:ss}]");
                await Reporter.WriteAsync(stdout, changes, options.Json);
            }

            previous = result.Files;
            await Task.Delay(TimeSpan.FromSeconds(options.IntervalSeconds), ct);
        }
    }

    private static Scanner BuildScanner(CliOptions options, string root)
    {
        var excludes = new List<string>(options.Excludes);

        // If the baseline file lives inside the scanned folder, never hash it.
        var baselineFull = Path.GetFullPath(options.BaselinePath);
        var relative = Path.GetRelativePath(root, baselineFull).Replace('\\', '/');
        if (!relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative))
            excludes.Add(relative);

        return new Scanner(excludes);
    }

    private static async Task ReportSkippedAsync(ScanResult result, TextWriter stderr)
    {
        foreach (var skipped in result.Skipped)
            await stderr.WriteLineAsync($"Warning: skipped {skipped}");
    }
}
