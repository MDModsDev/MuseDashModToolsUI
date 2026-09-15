using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;

namespace Euterpe.Tests.Models;

[Category("MapperTests")]
[TestSubject(typeof(Mapper))]
public sealed partial class MapperTest
{
    public static IEnumerable<Func<(Lib lib, LibDto expected)>> LibToModelCases()
    {
        yield return () => (
            new Lib
            {
                Slug = "test-lib",
                FileExtension = "dll",
                Versions = new Dictionary<string, DistributionVersion<LibMetadata>>
                {
                    ["1.0.0"] = new() { SHA256 = "abc123", DownloadUrl = "https://example.com/lib.dll" }
                }
            },
            new LibDto
            {
                Name = "test-lib",
                FileName = "test-lib.dll",
                SHA256 = "abc123",
                DownloadUrl = "https://example.com/lib.dll"
            });

        yield return () => (
            new Lib
            {
                Slug = "another",
                FileExtension = "zip",
                Versions = new Dictionary<string, DistributionVersion<LibMetadata>>
                {
                    ["2.5.1"] = new() { SHA256 = "def456", DownloadUrl = "https://cdn.example.com/another.zip" }
                }
            },
            new LibDto
            {
                Name = "another",
                FileName = "another.zip",
                SHA256 = "def456",
                DownloadUrl = "https://cdn.example.com/another.zip"
            });
    }

    [Test]
    [MethodDataSource(nameof(LibToModelCases))]
    public async Task ToModel_LibContract_ProjectsAllRelevantFields((Lib lib, LibDto expected) data)
    {
        var actual = data.lib.ToModel();

        using var _ = Assert.Multiple();
        await Assert.That(actual.Name).IsEqualTo(data.expected.Name);
        await Assert.That(actual.FileName).IsEqualTo(data.expected.FileName);
        await Assert.That(actual.SHA256).IsEqualTo(data.expected.SHA256);
        await Assert.That(actual.DownloadUrl).IsEqualTo(data.expected.DownloadUrl);
        await Assert.That(actual.IsLocal).IsFalse();
    }

    [Test]
    public async Task LibToModel_WithMultipleVersions_Throws()
    {
        var lib = new Lib
        {
            Slug = "multi",
            FileExtension = "dll",
            Versions = new Dictionary<string, DistributionVersion<LibMetadata>>
            {
                ["1.0.0"] = new(),
                ["2.0.0"] = new()
            }
        };

        Action act = () => lib.ToModel();
        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task ToModel_ModContract_PopulatesDtoFields()
    {
        var mod = new Mod
        {
            Mid = 42,
            Name = "TestMod",
            Version = "1.2.3",
            Author = "Tester",
            FileName = "TestMod.dll",
            Repository = "user/repo",
            ConfigFile = "TestMod.cfg",
            GameVersion = "1.0.0",
            MelonVersion = "0.5.0",
            Description = "A test mod",
            ModDependencies = ["DepA", "DepB"],
            LibDependencies = ["LibX"],
            IncompatibleMods = ["IncA"],
            SHA256 = "deadbeef",
            DownloadUrl = "https://example.com/TestMod.dll",
            DownloadCount = 100
        };

        var dto = mod.ToModel();

        using var _ = Assert.Multiple();
        await Assert.That(dto.Mid).IsEqualTo(mod.Mid);
        await Assert.That(dto.Name).IsEqualTo(mod.Name);
        await Assert.That(dto.Version).IsEqualTo(mod.Version);
        await Assert.That(dto.Author).IsEqualTo(mod.Author);
        await Assert.That(dto.FileName).IsEqualTo(mod.FileName);
        await Assert.That(dto.Repository).IsEqualTo(mod.Repository);
        await Assert.That(dto.ConfigFile).IsEqualTo(mod.ConfigFile);
        await Assert.That(dto.GameVersion).IsEqualTo(mod.GameVersion);
        await Assert.That(dto.MelonVersion).IsEqualTo(mod.MelonVersion);
        await Assert.That(dto.Description).IsEqualTo(mod.Description);
        await Assert.That(dto.ModDependencies).IsEquivalentTo(mod.ModDependencies, EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(dto.LibDependencies).IsEquivalentTo(mod.LibDependencies, EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(dto.IncompatibleMods).IsEquivalentTo(mod.IncompatibleMods, EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(dto.SHA256).IsEqualTo(mod.SHA256);
        await Assert.That(dto.DownloadUrl).IsEqualTo(mod.DownloadUrl);
        await Assert.That(dto.DownloadCount).IsEqualTo(mod.DownloadCount);
    }
}
