namespace FileGuard.Cli;

public static class ArgParser
{
    /// <summary>Parses command-line arguments. Throws ArgumentException with a friendly message on bad input.</summary>
    public static CliOptions Parse(string[] args)
    {
        var options = new CliOptions();

        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            options.ShowHelp = true;
            return options;
        }

        options.Command = args[0].ToLowerInvariant();

        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "--json":
                    options.Json = true;
                    break;

                case "-b":
                case "--baseline":
                    options.BaselinePath = NextValue(args, ref i, arg);
                    break;

                case "-e":
                case "--exclude":
                    options.Excludes.Add(NextValue(args, ref i, arg));
                    break;

                case "-i":
                case "--interval":
                {
                    var raw = NextValue(args, ref i, arg);
                    if (!int.TryParse(raw, out var seconds) || seconds < 1)
                        throw new ArgumentException("--interval must be a positive whole number of seconds.");
                    options.IntervalSeconds = seconds;
                    break;
                }

                default:
                    if (arg.StartsWith('-'))
                        throw new ArgumentException($"Unknown option: {arg}");
                    if (options.Path is not null)
                        throw new ArgumentException($"Unexpected extra argument: {arg}");
                    options.Path = arg;
                    break;
            }
        }

        return options;
    }

    private static string NextValue(string[] args, ref int i, string option)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Option {option} requires a value.");
        return args[++i];
    }
}
