using FileGuard.Cli;

namespace FileGuard.Tests;

public class ArgParserTests
{
    [Fact]
    public void No_arguments_shows_help()
    {
        Assert.True(ArgParser.Parse(Array.Empty<string>()).ShowHelp);
    }

    [Fact]
    public void Parses_command_path_and_options()
    {
        var o = ArgParser.Parse(new[]
        {
            "init", "/tmp/data", "-b", "base.json", "-e", "*.log", "-e", "bin/**", "--json", "-i", "5"
        });

        Assert.Equal("init", o.Command);
        Assert.Equal("/tmp/data", o.Path);
        Assert.Equal("base.json", o.BaselinePath);
        Assert.Equal(new[] { "*.log", "bin/**" }, o.Excludes);
        Assert.True(o.Json);
        Assert.Equal(5, o.IntervalSeconds);
    }

    [Fact]
    public void Unknown_option_throws()
    {
        Assert.Throws<ArgumentException>(() => ArgParser.Parse(new[] { "check", "/tmp", "--nope" }));
    }

    [Fact]
    public void Option_without_value_throws()
    {
        Assert.Throws<ArgumentException>(() => ArgParser.Parse(new[] { "check", "/tmp", "--baseline" }));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("abc")]
    public void Invalid_interval_throws(string value)
    {
        Assert.Throws<ArgumentException>(() => ArgParser.Parse(new[] { "watch", "/tmp", "-i", value }));
    }
}
