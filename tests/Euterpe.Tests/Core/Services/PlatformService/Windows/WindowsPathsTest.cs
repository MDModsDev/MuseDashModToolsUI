using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Core;

[Category("WindowsPathsTests")]
[TestSubject(typeof(WindowsPaths))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsPathsTest
{
    [Test]
    public async Task SteamSearch_WindowsLocations_CombinesDrivesAndKnownFolders()
    {
        var drives = Environment.GetLogicalDrives();
        var expectedFolders = new[]
        {
            @"Program Files\Steam",
            @"Program Files (x86)\Steam",
            @"Program Files\SteamLibrary",
            @"Program Files (x86)\SteamLibrary",
            "Steam",
            "SteamLibrary"
        };

        using var _ = Assert.Multiple();
        await Assert.That(WindowsPaths.SteamSearch.Length).IsEqualTo(drives.Length * expectedFolders.Length);

        foreach (var drive in drives)
        {
            foreach (var folder in expectedFolders)
            {
                await Assert.That(WindowsPaths.SteamSearch).Contains(Path.Combine(drive, folder));
            }
        }
    }
}
