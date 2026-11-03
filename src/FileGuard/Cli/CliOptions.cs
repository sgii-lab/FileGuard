namespace FileGuard.Cli;

public class CliOptions
{
    public string Command { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string BaselinePath { get; set; } = "fileguard-baseline.json";
    public List<string> Excludes { get; } = new();
    public bool Json { get; set; }
    public int IntervalSeconds { get; set; } = 30;
    public bool ShowHelp { get; set; }
}
