namespace FileGuard.Cli;

public static class HelpText
{
    public const string Usage = """
        FileGuard - file integrity monitor

        USAGE
          fileguard <command> <path> [options]

        COMMANDS
          init    Hash every file under <path> and save a baseline
          check   Compare <path> against the baseline and report changes
          watch   Re-check repeatedly until Ctrl+C

        OPTIONS
          -b, --baseline <file>   Baseline file (default: fileguard-baseline.json)
          -e, --exclude <glob>    Skip files matching a glob (repeatable)
                                  e.g. -e "*.log" -e ".git/**"
          -i, --interval <secs>   Seconds between scans in watch mode (default: 30)
              --json              Print results as JSON
          -h, --help              Show this help

        EXIT CODES
          0  no changes      1  changes detected      2  error

        EXAMPLES
          fileguard init /etc -e "*.log"
          fileguard check /etc -e "*.log"
          fileguard watch ~/projects --interval 10
        """;
}
