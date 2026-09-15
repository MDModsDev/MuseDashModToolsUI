using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("WindowsSteamPathDiscoveryTests")]
[TestSubject(typeof(WindowsSteamPathDiscovery))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsSteamPathDiscoveryTest
{
    private readonly MockLogger<WindowsSteamPathDiscovery> _logger = Mock.Logger<WindowsSteamPathDiscovery>();
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"WindowsSteamPathDiscoveryTest_{Guid.NewGuid():N}");
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

    private WindowsSteamPathDiscovery CreateService(string steamFolder = "") => new()
    {
        Config = new Config { SteamFolder = steamFolder, MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() },
        Logger = _logger
    };

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
    public async Task GetSteamExecPathAsync_FileExists_ReturnsPath()
    {
        var execPath = Path.Combine(_tempDir, "steam.exe");
        await File.WriteAllBytesAsync(execPath, []);

        var service = CreateService(_tempDir);
        var result = await service.GetSteamExecPathAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsEqualTo(execPath);
        _logger.VerifyLog().ContainingMessage($"steam.exe found at: {execPath}");
    }

    [Test]
    public async Task GetSteamExecPathAsync_FileMissing_ReturnsNull()
    {
        var service = CreateService(_tempDir);
        var result = await service.GetSteamExecPathAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsNull();
        _logger.VerifyLog().ContainingMessage("steam.exe not found");
    }

    [Test]
    public async Task CheckIsValidSteamExecPath_MissingFile_ReturnsFalse()
    {
        var service = CreateService();
        var path = Path.Combine(_tempDir, "missing.exe");
        await Assert.That(service.CheckIsValidSteamExecPath(path)).IsFalse();
    }

    [Test]
    public async Task CheckIsValidSteamExecPath_EmptyFile_ReturnsFalse()
    {
        // FileVersionInfo on a non-PE file returns null company/product → returns false.
        var path = Path.Combine(_tempDir, "fake.exe");
        await File.WriteAllBytesAsync(path, []);

        var service = CreateService();
        await Assert.That(service.CheckIsValidSteamExecPath(path)).IsFalse();
    }

    [Test]
    public async Task TryGetSteamFolder_CurrentEnvironment_ReturnsExistingFolderOrNull()
    {
        // SteamSearch is computed once from logical drives and Steam-related paths.
        // On CI Windows runner Steam isn't installed, so this should return false; on a
        // dev machine it might find Steam — both branches are valid.
        var service = CreateService();
        var found = service.TryGetSteamFolder(out var folder);

        if (found)
        {
            using var _ = Assert.Multiple();
            await Assert.That(folder).IsNotNull();
            await Assert.That(Directory.Exists(folder)).IsTrue();
        }
        else
        {
            await Assert.That(folder).IsNull();
        }
    }
}
