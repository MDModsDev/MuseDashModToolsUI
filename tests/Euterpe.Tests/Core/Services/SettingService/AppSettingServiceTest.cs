using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Core;

[Category("AppSettingServiceTests")]
[TestSubject(typeof(AppSettingService))]
public sealed class AppSettingServiceTest
{
    [Test]
    public async Task ValidateSteamAsync_AllValid_DoesNotPromptOrChangeConfig()
    {
        var config = NewConfig();
        config.SteamFolder = "/steam";
        config.SteamExecPath = "/steam/steam";
        var steam = NewSteamMock(true, true);
        var msgBox = IMessageBoxService.Mock();

        var service = NewService(config, steam, msgBox);
        await service.ValidateSteamAsync();

        msgBox.NoticeOverlayAsync(Any<string>()).WasNeverCalled();
        using var assertions = Assert.Multiple();
        await Assert.That(config.SteamFolder).IsEqualTo("/steam");
        await Assert.That(config.SteamExecPath).IsEqualTo("/steam/steam");
    }

    [Test]
    public async Task ValidateSteamAsync_InvalidFolderAutoDetected_WritesToConfig()
    {
        var config = NewConfig();
        config.SteamFolder = "/bad";
        config.SteamExecPath = "/steam/steam";
        var steam = NewSteamMock(false, true, "/auto/steam");
        var msgBox = IMessageBoxService.Mock();

        var service = NewService(config, steam, msgBox);
        await service.ValidateSteamAsync();

        msgBox.NoticeOverlayAsync(Any<string>()).WasNeverCalled();
        await Assert.That(config.SteamFolder).IsEqualTo("/auto/steam");
    }

    [Test]
    public async Task ValidateSteamAsync_InvalidFolderNotDetected_PromptsAndUsesUserPick()
    {
        var config = NewConfig();
        config.SteamFolder = "/bad";
        config.SteamExecPath = "/steam/steam";
        var steam = NewSteamMock(false, true);
        var msgBox = IMessageBoxService.Mock();
        var appLocal = IAppLocalService.Mock();
        appLocal.GetSteamFolderAsync().Returns("/user/picked");

        var service = NewService(config, steam, msgBox, appLocal);
        await service.ValidateSteamAsync();

        msgBox.NoticeOverlayAsync(Any<string>()).WasCalled(Times.Once);
        await Assert.That(config.SteamFolder).IsEqualTo("/user/picked");
    }

    [Test]
    public async Task ValidateSteamAsync_InvalidExecPathAutoDetected_WritesToConfig()
    {
        var config = NewConfig();
        config.SteamFolder = "/steam";
        config.SteamExecPath = "/bad";
        var steam = NewSteamMock(true, false, detectedExecPath: "/auto/steam.exe");

        var service = NewService(config, steam);
        await service.ValidateSteamAsync();

        await Assert.That(config.SteamExecPath).IsEqualTo("/auto/steam.exe");
    }

    private static ISteamPathDiscovery NewSteamMock(
        bool folderValid,
        bool execValid,
        string? detectedFolder = null,
        string? detectedExecPath = null)
    {
        var steam = ISteamPathDiscovery.Mock();
        steam.CheckIsValidSteamFolder(Any<string>()).Returns(folderValid);
        steam.CheckIsValidSteamExecPath(Any<string>()).Returns(execValid);
        steam.TryGetSteamFolder()
            .SetsOutSteamFolder(detectedFolder)
            .Returns(detectedFolder is not null);
        steam.GetSteamExecPathAsync().Returns(detectedExecPath);
        return steam;
    }

    private static Config NewConfig() =>
        new() { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() };

    private static AppSettingService NewService(
        Config config,
        ISteamPathDiscovery steam,
        IMessageBoxService? msgBox = null,
        IAppLocalService? appLocal = null) => new()
    {
        Config = config,
        SteamDiscovery = steam,
        MessageBoxService = msgBox ?? IMessageBoxService.Mock(),
        AppLocalService = appLocal ?? IAppLocalService.Mock(),
        JsonSerializationService = IJsonSerializationService.Mock(),
        Logger = NullLogger<AppSettingService>.Instance
    };
}
