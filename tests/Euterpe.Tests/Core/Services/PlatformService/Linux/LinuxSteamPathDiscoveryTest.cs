using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("LinuxSteamPathDiscoveryTests")]
[TestSubject(typeof(LinuxSteamPathDiscovery))]
[RunOn(OS.Linux)]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
public sealed class LinuxSteamPathDiscoveryTest
{
    private readonly MockLogger<LinuxSteamPathDiscovery> _logger = Mock.Logger<LinuxSteamPathDiscovery>();
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"LinuxSteamPathDiscoveryTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [After(Test)]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private LinuxSteamPathDiscovery CreateService() => new() { Logger = _logger };

    [Test]
    public async Task CheckIsValidSteamFolder_SteamAppsExists_ReturnsTrue()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "steamapps"));

        var service = CreateService();
        var result = service.CheckIsValidSteamFolder(_tempDir);

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        _logger.VerifyLog().ContainingMessage($"Valid Steam folder: {_tempDir}");
    }

    [Test]
    public async Task CheckIsValidSteamFolder_NoSteamApps_ReturnsFalse()
    {
        var service = CreateService();
        var result = service.CheckIsValidSteamFolder(_tempDir);

        using var _ = Assert.Multiple();
        await Assert.That(result).IsFalse();
        _logger.VerifyLog().ContainingMessage($"Invalid Steam folder: {_tempDir}");
    }

    [Test]
    public async Task CheckIsValidSteamExecPath_FileWithExecutePermission_ReturnsTrue()
    {
        var path = Path.Combine(_tempDir, "steam");
        await File.WriteAllBytesAsync(path, []);
        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        var service = CreateService();
        await Assert.That(service.CheckIsValidSteamExecPath(path)).IsTrue();
    }

    [Test]
    public async Task CheckIsValidSteamExecPath_FileWithoutExecutePermission_ReturnsFalse()
    {
        var path = Path.Combine(_tempDir, "not-exec");
        await File.WriteAllBytesAsync(path, []);
        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);

        var service = CreateService();
        await Assert.That(service.CheckIsValidSteamExecPath(path)).IsFalse();
    }

    [Test]
    public async Task CheckIsValidSteamExecPath_MissingFile_ReturnsFalse()
    {
        var service = CreateService();
        var path = Path.Combine(_tempDir, "missing");
        await Assert.That(service.CheckIsValidSteamExecPath(path)).IsFalse();
    }

    [Test]
    public async Task TryGetSteamFolder_CurrentFileSystem_ReturnsExistingSteamFolderOrNull()
    {
        // SteamSearch is initialized from HOME at type init, so we can't redirect it.
        // Just exercise both branches: on CI Linux (no Steam) → false; on a dev machine with Steam → true.
        var service = CreateService();
        var found = service.TryGetSteamFolder(out var folder);

        if (found)
        {
            using var _ = Assert.Multiple();
            await Assert.That(folder).IsNotNull();
            await Assert.That(Directory.Exists(folder)).IsTrue();
            await Assert.That(Directory.Exists(Path.Combine(folder!, "steamapps"))).IsTrue();
        }
        else
        {
            await Assert.That(folder).IsNull();
        }
    }

    [Test]
    public async Task GetSteamExecPathAsync_WhichStub_ReturnsNullWhenNotFound()
    {
        // CI Linux runner has no `steam` binary → `which steam` exits non-zero → returns null.
        // (If `steam` happens to be installed, this asserts the path actually exists — still valid.)
        var service = CreateService();
        var result = await service.GetSteamExecPathAsync();

        if (result is null)
        {
            return;
        }

        await Assert.That(File.Exists(result)).IsTrue();
    }
}
