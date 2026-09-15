using Euterpe.Contracts.Mods;

namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task FindModByName_BeforeInit_ReturnsNull()
    {
        var sut = CreateModManageService();
        await Assert.That(sut.FindModByName(TestModName)).IsNull();
    }

    [Test]
    public async Task InitializeModsAsync_CalledTwice_OnlyLoadsOnce()
    {
        var localServiceMock = IModLocalService.Mock();
        localServiceMock.GetModFilePaths().Returns([]);
        localServiceMock.GetLibFilePaths().Returns([]);

        var sut = CreateModManageService(modLocalService: localServiceMock);

        await sut.InitializeModsAsync();
        await sut.InitializeModsAsync();

        using var _ = Assert.Multiple();
        localServiceMock.GetModFilePaths().WasCalled(Times.Once);
        localServiceMock.GetLibFilePaths().WasCalled(Times.Once);
    }

    [Test]
    public async Task InitializeModsAsync_LocalModsAvailable_LoadsThemIntoSourceCache()
    {
        var localServiceMock = IModLocalService.Mock();
        localServiceMock.GetModFilePaths().Returns([TestModFilePath]);
        localServiceMock.GetLibFilePaths().Returns([]);
        localServiceMock.LoadModFromPathAsync(TestModFilePath).Returns(CreateInstalledMod());

        var sut = CreateModManageService(modLocalService: localServiceMock);

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)).IsNotNull();
    }

    [Test]
    public async Task InitializeModsAsync_WebOnlyModsAvailable_AddsThemToSourceCache()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns(
        [
            new Mod
            {
                Name = "WebMod",
                Version = "2.0.0",
                FileName = "WebMod.dll",
                GameVersion = "*"
            }
        ]);

        var sut = CreateModManageService(gameDownloadManager: downloadManagerMock);

        await sut.InitializeModsAsync();

        var webMod = sut.FindModByName("WebMod");
        using var _ = Assert.Multiple();
        await Assert.That(webMod).IsNotNull();
        await Assert.That(webMod!.IsLocal).IsFalse();
        await Assert.That(webMod.HasDownloadSource).IsTrue();
    }
}
