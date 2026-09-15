namespace Euterpe.Tests.Core;

[Category("GameSettingServiceTests")]
[TestSubject(typeof(GameSettingService))]
public sealed class GameSettingServiceTest
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task IsValidGameFolder_ConfiguredGameFolder_DelegatesToGamePathDiscovery(bool expected)
    {
        var paths = IGamePathDiscovery.Mock();
        paths.CheckIsValidGameFolder(Any<string?>()).Returns(expected);
        var service = new GameSettingService
        {
            GameConfig = new MuseDashConfig { Folder = "/some/folder" },
            GamePaths = paths
        };

        await Assert.That(service.IsValidGameFolder()).IsEqualTo(expected);
    }

    [Test]
    public async Task EnsureGameFolders_RequiredDirectoriesMissing_CreatesAllRequiredDirectories()
    {
        var root = Path.Combine(Path.GetTempPath(), $"euterpe-test-{Guid.NewGuid():N}");
        try
        {
            var gameConfig = new MuseDashConfig { Folder = root };
            var service = new GameSettingService
            {
                GameConfig = gameConfig,
                GamePaths = IGamePathDiscovery.Mock()
            };

            service.EnsureGameFolders();

            using var assertions = Assert.Multiple();
            await Assert.That(Directory.Exists(gameConfig.ModsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.UserLibsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.OnlineChartsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.OfflineChartsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.TempChartsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.TempModsFolder)).IsTrue();
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Test]
    public async Task EnsureGameFolders_TempDownloadInProgress_PreservesInFlightContent()
    {
        var root = Path.Combine(Path.GetTempPath(), $"euterpe-test-{Guid.NewGuid():N}");
        try
        {
            var gameConfig = new MuseDashConfig { Folder = root };
            var service = new GameSettingService
            {
                GameConfig = gameConfig,
                GamePaths = IGamePathDiscovery.Mock()
            };

            var workFolder = Path.Combine(gameConfig.TempChartsFolder, "13");
            Directory.CreateDirectory(workFolder);
            var inFlightFile = Path.Combine(workFolder, "manifest.epk");
            await File.WriteAllTextAsync(inFlightFile, "partial");

            service.EnsureGameFolders();

            // EnsureGameFolders must not wipe temp, or it races with the active download.
            using var assertions = Assert.Multiple();
            await Assert.That(File.Exists(inFlightFile)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.TempModsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.TempChartsFolder)).IsTrue();
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
