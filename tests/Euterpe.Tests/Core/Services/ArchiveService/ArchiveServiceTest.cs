using System.IO.Compression;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Core;

[Category("ArchiveServiceTests")]
[TestSubject(typeof(ArchiveService))]
public sealed class ArchiveServiceTest
{
    private static ArchiveService NewService() => new() { Logger = NullLogger<ArchiveService>.Instance };

    private static string NewTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "Euterpe.Tests.Archive_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [Test]
    public async Task CreateZipFile_NewArchive_WritesValidZipContainingFiles()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "a.txt"), "alpha");
            await File.WriteAllTextAsync(Path.Combine(source, "b.txt"), "beta");
            var zipPath = Path.Combine(work, "out.zip");

            NewService().CreateZipFile(source, zipPath);

            await using var zip = ZipFile.OpenRead(zipPath);
            using var _ = Assert.Multiple();
            await Assert.That(File.Exists(zipPath)).IsTrue();
            await Assert.That(zip.Entries.Select(e => e.Name).Order(StringComparer.Ordinal))
                .IsEquivalentTo(["a.txt", "b.txt"], StringComparer.Ordinal, CollectionOrdering.Matching);
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task CreateZipFile_ExistingZip_OverwritesIt()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "new.txt"), "new");
            var zipPath = Path.Combine(work, "out.zip");
            await File.WriteAllTextAsync(zipPath, "stale-content");

            NewService().CreateZipFile(source, zipPath);

            await using var zip = ZipFile.OpenRead(zipPath);
            await Assert.That(zip.Entries.Single().Name).IsEqualTo("new.txt");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task CreateZipFileAsync_NewArchive_WritesValidZip()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "a.txt"), "alpha");
            var zipPath = Path.Combine(work, "out.zip");

            await NewService().CreateZipFileAsync(source, zipPath);

            await using var zip = ZipFile.OpenRead(zipPath);
            using var _ = Assert.Multiple();
            await Assert.That(File.Exists(zipPath)).IsTrue();
            await Assert.That(zip.Entries.Single().Name).IsEqualTo("a.txt");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task CreateZipFileAsync_ExistingZip_OverwritesIt()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "fresh.txt"), "fresh");
            var zipPath = Path.Combine(work, "out.zip");
            await File.WriteAllTextAsync(zipPath, "stale");

            await NewService().CreateZipFileAsync(source, zipPath);

            await using var zip = ZipFile.OpenRead(zipPath);
            await Assert.That(zip.Entries.Single().Name).IsEqualTo("fresh.txt");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task ExtractZipFile_ArchiveWithTextFile_RestoresOriginalContents()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "hello.txt"), "world");
            var zipPath = Path.Combine(work, "out.zip");
            ZipFile.CreateFromDirectory(source, zipPath);
            var dest = Path.Combine(work, "out");

            NewService().ExtractZipFile(zipPath, dest);

            await Assert.That(await File.ReadAllTextAsync(Path.Combine(dest, "hello.txt"))).IsEqualTo("world");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task ExtractZipFile_DestinationFileExists_OverwritesExistingFile()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "hello.txt"), "new-value");
            var zipPath = Path.Combine(work, "out.zip");
            ZipFile.CreateFromDirectory(source, zipPath);
            var dest = Path.Combine(work, "out");
            Directory.CreateDirectory(dest);
            await File.WriteAllTextAsync(Path.Combine(dest, "hello.txt"), "old-value");

            NewService().ExtractZipFile(zipPath, dest);

            await Assert.That(await File.ReadAllTextAsync(Path.Combine(dest, "hello.txt"))).IsEqualTo("new-value");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task ExtractZipFileAsync_ArchiveWithTextFile_RestoresOriginalContents()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "hello.txt"), "async-world");
            var zipPath = Path.Combine(work, "out.zip");
            ZipFile.CreateFromDirectory(source, zipPath);
            var dest = Path.Combine(work, "out");

            await NewService().ExtractZipFileAsync(zipPath, dest);

            await Assert.That(await File.ReadAllTextAsync(Path.Combine(dest, "hello.txt"))).IsEqualTo("async-world");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }
}
