using FileGuard.Core;

namespace FileGuard.Tests;

public class ComparerTests
{
    private static FileEntry F(string path, string hash) => new(path, hash, 1);

    [Fact]
    public void Identical_snapshots_have_no_changes()
    {
        var snapshot = new[] { F("a.txt", "1"), F("b.txt", "2") };
        Assert.Empty(BaselineComparer.Compare(snapshot, snapshot));
    }

    [Fact]
    public void Detects_added_modified_and_deleted()
    {
        var before = new[] { F("keep.txt", "1"), F("edit.txt", "2"), F("gone.txt", "3") };
        var after = new[] { F("keep.txt", "1"), F("edit.txt", "CHANGED"), F("new.txt", "4") };

        var changes = BaselineComparer.Compare(before, after);

        Assert.Equal(3, changes.Count);
        Assert.Contains(new Change(ChangeType.Modified, "edit.txt"), changes);
        Assert.Contains(new Change(ChangeType.Deleted, "gone.txt"), changes);
        Assert.Contains(new Change(ChangeType.Added, "new.txt"), changes);
    }
}
