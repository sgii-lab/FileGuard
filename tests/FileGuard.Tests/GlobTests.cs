using FileGuard.Core;

namespace FileGuard.Tests;

public class GlobTests
{
    [Theory]
    [InlineData("*.log", "app.log", true)]
    [InlineData("*.log", "logs/app.log", true)]
    [InlineData("*.log", "app.txt", false)]
    [InlineData("bin/**", "bin/Debug/app.dll", true)]
    [InlineData("bin/**", "src/bin.cs", false)]
    [InlineData(".git/**", ".git/config", true)]
    [InlineData("docs/*.md", "docs/readme.md", true)]
    [InlineData("docs/*.md", "docs/deep/readme.md", false)]
    [InlineData("file?.txt", "file1.txt", true)]
    [InlineData("file?.txt", "file12.txt", false)]
    public void Matches_as_expected(string pattern, string path, bool expected)
    {
        Assert.Equal(expected, Glob.IsMatch(pattern, path));
    }
}
