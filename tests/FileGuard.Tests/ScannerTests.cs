using FileGuard.Core;

namespace FileGuard.Tests;

public class ScannerTests
{
    [Fact]
    public async Task Computes_correct_sha256()
    {
        using var dir = new TempDir();
        dir.Write("hello.txt", "hello");

        var result = await new Scanner().ScanAsync(dir.Root);

        var entry = Assert.Single(result.Files);
        Assert.Equal("hello.txt", entry.Path);
        Assert.Equal("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", entry.Hash);
        Assert.Equal(5, entry.Size);
    }

    [Fact]
    public async Task Scans_subdirectories_with_forward_slash_paths()
    {
        using var dir = new TempDir();
        dir.Write("top.txt", "a");
        dir.Write("sub/deeper/file.txt", "b");

        var result = await new Scanner().ScanAsync(dir.Root);

        Assert.Equal(new[] { "sub/deeper/file.txt", "top.txt" }, result.Files.Select(f => f.Path).ToArray());
    }

    [Fact]
    public async Task Respects_exclude_patterns()
    {
        using var dir = new TempDir();
        dir.Write("keep.txt", "a");
        dir.Write("noise.log", "b");
        dir.Write("bin/out.dll", "c");

        var result = await new Scanner(new[] { "*.log", "bin/**" }).ScanAsync(dir.Root);

        Assert.Equal(new[] { "keep.txt" }, result.Files.Select(f => f.Path).ToArray());
    }

    [Fact]
    public async Task Missing_directory_throws()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => new Scanner().ScanAsync(missing));
    }
}
