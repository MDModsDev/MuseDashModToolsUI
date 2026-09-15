using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Core;

[Category("LinuxPathsTests")]
[TestSubject(typeof(LinuxPaths))]
[RunOn(OS.Linux)]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
public sealed class LinuxPathsTest
{
    [Test]
    public async Task SteamSearch_LinuxLocations_AreRootedUnderUserProfile()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        await Assert.That(LinuxPaths.SteamSearch).IsNotEmpty();
        foreach (var path in LinuxPaths.SteamSearch)
        {
            await Assert.That(path).StartsWith(home);
        }
    }

    [Test]
    public async Task SteamSearch_LinuxLocations_ContainsKnownLocations()
    {
        var joined = string.Join(';', LinuxPaths.SteamSearch);

        using var _ = Assert.Multiple();
        await Assert.That(joined).Contains(".local/share/Steam");
        await Assert.That(joined).Contains(".steam/steam");
        await Assert.That(joined).Contains(".var/app/com.valvesoftware.Steam/data/Steam");
        await Assert.That(joined).Contains(".steam/root");
    }

    [Test]
    public async Task SteamSearch_LinuxLocations_HasNoDuplicateEntries()
    {
        await Assert.That(LinuxPaths.SteamSearch).HasDistinctItems();
    }
}
