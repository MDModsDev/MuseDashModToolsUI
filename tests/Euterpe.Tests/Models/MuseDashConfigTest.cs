using Semver;

namespace Euterpe.Tests.Models;

[Category("MuseDashConfigTests")]
[TestSubject(typeof(MuseDashConfig))]
public sealed class MuseDashConfigTest
{
    private const string GameFolder = "/games/MuseDash";

    [Test]
    public async Task ComputedPaths_GameFolderConfigured_AreDerivedFromFolder()
    {
        var game = new MuseDashConfig { Folder = GameFolder };

        using var _ = Assert.Multiple();
        await Assert.That(game.ModsFolder).IsEqualTo(Path.Combine(GameFolder, "Mods"));
        await Assert.That(game.UserDataFolder).IsEqualTo(Path.Combine(GameFolder, "UserData"));
        await Assert.That(game.UserLibsFolder).IsEqualTo(Path.Combine(GameFolder, "UserLibs"));
        await Assert.That(game.MelonLoaderFolder).IsEqualTo(Path.Combine(GameFolder, "MelonLoader"));
        await Assert.That(game.MelonLoaderZipPath).IsEqualTo(Path.Combine(GameFolder, "MelonLoader.zip"));
        await Assert.That(game.LatestLogPath).IsEqualTo(Path.Combine(GameFolder, "MelonLoader", "Latest.log"));
    }

    [Test]
    public async Task ChartsFolders_GameFolderConfigured_AreNestedUnderEuterpeChartsFolder()
    {
        var game = new MuseDashConfig { Folder = GameFolder };

        using var _ = Assert.Multiple();
        await Assert.That(game.OnlineChartsFolder).IsEqualTo(Path.Combine(GameFolder, "Euterpe_Charts", "Online"));
        await Assert.That(game.OfflineChartsFolder).IsEqualTo(Path.Combine(GameFolder, "Euterpe_Charts", "Offline"));
    }

    [Test]
    public async Task GameDataFolder_GameFolderConfigured_UsesGameDataFolderName()
    {
        var game = new MuseDashConfig { Folder = GameFolder };
        await Assert.That(game.GameDataFolder).IsEqualTo(Path.Combine(GameFolder, "MuseDash_Data"));
    }

    [Test]
    public async Task GameMode_NewConfig_IsModded() =>
        await Assert.That(new MuseDashConfig().GameMode).IsEqualTo(GameMode.Modded);

    [Test]
    public async Task UnityDependencyZipPath_UnityVersionProvided_IncludesVersion()
    {
        var game = new MuseDashConfig { Folder = GameFolder };

        await Assert.That(game.UnityDependencyZipPath("2019.4.32")).IsEqualTo(
            Path.Combine(GameFolder, "MelonLoader", "Dependencies", "Il2CppAssemblyGenerator", "UnityDependencies_2019.4.32.zip"));
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("not-a-version")]
    public async Task SettingMelonLoaderVersion_InvalidString_LeavesSemVersionNull(string? input)
    {
        var game = new MuseDashConfig { MelonLoaderVersion = input };
        await Assert.That(game.MelonLoaderSemVersion).IsNull();
    }

    [Test]
    [Arguments("0.5.7")]
    [Arguments("1.0.0")]
    [Arguments("0.6.0-rc1")]
    public async Task SettingMelonLoaderVersion_ValidString_ParsesSemVersion(string input)
    {
        var game = new MuseDashConfig { MelonLoaderVersion = input };

        using var _ = Assert.Multiple();
        await Assert.That(game.MelonLoaderSemVersion).IsNotNull();
        await Assert.That(game.MelonLoaderSemVersion).IsEqualTo(SemVersion.Parse(input));
    }
}
