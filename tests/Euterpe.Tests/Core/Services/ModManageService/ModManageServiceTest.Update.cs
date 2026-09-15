namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task UpdateModAsync_WhenAllSucceed_ReplacesFileAndNotifiesSuccess()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.UpdateModAsync(mod);

        using var _ = Assert.Multiple();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Once);
        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Never);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task UpdateModAsync_WhenPreviousFileDisabled_RemovesStaleLeftover()
    {
        var mod = CreateInstalledMod(disabled: true);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.UpdateModAsync(mod);

        using var _ = Assert.Multiple();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Once);
        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task UpdateAllModsAsync_OutdatedAndCurrentMods_UpdatesOnlyOutdatedMods()
    {
        var outdated = CreateInstalledMod("Outdated", "Outdated.dll");
        var upToDate = CreateInstalledMod("UpToDate", "UpToDate.dll");
        upToDate.LocalSHA256 = "shared-sha";

        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).Returns(true);

        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns(
        [
            CreateWebMod("Outdated", "2.0.0"),
            CreateWebMod("UpToDate", "1.0.0", "shared-sha")
        ]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(
                ("/mods/Outdated.dll", outdated),
                ("/mods/UpToDate.dll", upToDate)),
            notificationService: notificationServiceMock);

        await sut.InitializeModsAsync();
        await sut.UpdateAllModsAsync();

        using var _ = Assert.Multiple();
        downloadManagerMock.DownloadModAsync(outdated, Any<CancellationToken>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(upToDate, Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UpdateAllModsAsync_WhenNoOutdatedMods_DoesNotDownload()
    {
        var upToDate = CreateInstalledMod();
        upToDate.LocalSHA256 = "shared-sha";

        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns(
        [
            CreateWebMod(sha256: "shared-sha")
        ]);

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            modLocalService: LocalServiceWith((TestModFilePath, upToDate)));

        await sut.InitializeModsAsync();
        await sut.UpdateAllModsAsync();

        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UpdateModAsync_WhenDownloadFails_NotifiesError()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Throws(new InvalidOperationException("download failed"));
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.UpdateModAsync(mod);

        using var _ = Assert.Multiple();
        fileSystemServiceMock.TryDeleteFile(Any<string>()).WasCalled(Times.Never);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
    }
}
