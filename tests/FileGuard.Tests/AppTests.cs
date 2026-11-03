using FileGuard.Cli;

namespace FileGuard.Tests;

/// <summary>End-to-end tests that drive the CLI exactly as a user would.</summary>
public class AppTests
{
    private static async Task<(int Code, string Out, string Err)> RunAsync(params string[] args)
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var code = await App.RunAsync(args, stdout, stderr);
        return (code, stdout.ToString(), stderr.ToString());
    }

    [Fact]
    public async Task Unchanged_directory_is_clean()
    {
        using var data = new TempDir();
        using var meta = new TempDir();
        data.Write("a.txt", "one");
        data.Write("sub/b.txt", "two");
        var baseline = Path.Combine(meta.Root, "baseline.json");

        var init = await RunAsync("init", data.Root, "-b", baseline);
        Assert.Equal(App.ExitClean, init.Code);

        var check = await RunAsync("check", data.Root, "-b", baseline);
        Assert.Equal(App.ExitClean, check.Code);
        Assert.Contains("no changes", check.Out);
    }

    [Fact]
    public async Task Reports_modified_added_and_deleted_files()
    {
        using var data = new TempDir();
        using var meta = new TempDir();
        var edited = data.Write("edit.txt", "before");
        var removed = data.Write("remove.txt", "bye");
        var baseline = Path.Combine(meta.Root, "baseline.json");

        await RunAsync("init", data.Root, "-b", baseline);

        File.WriteAllText(edited, "after");
        File.Delete(removed);
        data.Write("new.txt", "hello");

        var check = await RunAsync("check", data.Root, "-b", baseline);

        Assert.Equal(App.ExitChangesFound, check.Code);
        Assert.Contains("modified", check.Out);
        Assert.Contains("edit.txt", check.Out);
        Assert.Contains("deleted", check.Out);
        Assert.Contains("remove.txt", check.Out);
        Assert.Contains("added", check.Out);
        Assert.Contains("new.txt", check.Out);
    }

    [Fact]
    public async Task Json_output_uses_enum_names()
    {
        using var data = new TempDir();
        using var meta = new TempDir();
        var file = data.Write("a.txt", "before");
        var baseline = Path.Combine(meta.Root, "baseline.json");

        await RunAsync("init", data.Root, "-b", baseline);
        File.WriteAllText(file, "after");

        var check = await RunAsync("check", data.Root, "-b", baseline, "--json");

        Assert.Equal(App.ExitChangesFound, check.Code);
        Assert.Contains("\"Modified\"", check.Out);
        Assert.Contains("\"totalChanges\": 1", check.Out);
    }

    [Fact]
    public async Task Excluded_files_are_ignored()
    {
        using var data = new TempDir();
        using var meta = new TempDir();
        data.Write("a.txt", "stable");
        var log = data.Write("app.log", "v1");
        var baseline = Path.Combine(meta.Root, "baseline.json");

        await RunAsync("init", data.Root, "-b", baseline, "-e", "*.log");
        File.WriteAllText(log, "v2");

        var check = await RunAsync("check", data.Root, "-b", baseline, "-e", "*.log");
        Assert.Equal(App.ExitClean, check.Code);
    }

    [Fact]
    public async Task Baseline_inside_scanned_folder_is_not_hashed()
    {
        using var data = new TempDir();
        data.Write("a.txt", "stable");
        var baseline = Path.Combine(data.Root, "baseline.json");

        await RunAsync("init", data.Root, "-b", baseline);
        var check = await RunAsync("check", data.Root, "-b", baseline);

        Assert.Equal(App.ExitClean, check.Code);
    }

    [Fact]
    public async Task Check_without_baseline_is_an_error()
    {
        using var data = new TempDir();
        var missing = Path.Combine(data.Root, "nope.json");

        var check = await RunAsync("check", data.Root, "-b", missing);

        Assert.Equal(App.ExitError, check.Code);
        Assert.Contains("init", check.Err);
    }

    [Fact]
    public async Task Unknown_command_is_an_error()
    {
        var result = await RunAsync("explode", "/tmp");
        Assert.Equal(App.ExitError, result.Code);
        Assert.Contains("unknown command", result.Err);
    }

    [Fact]
    public async Task Missing_path_is_an_error()
    {
        var result = await RunAsync("init");
        Assert.Equal(App.ExitError, result.Code);
    }
}
