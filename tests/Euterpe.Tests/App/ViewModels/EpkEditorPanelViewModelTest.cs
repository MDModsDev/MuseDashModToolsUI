using Euterpe.Features.Charting;

namespace Euterpe.Tests.App.ViewModels;

[Category("EpkEditorPanelViewModelTests")]
[TestSubject(typeof(EpkEditorPanelViewModel))]
public sealed class EpkEditorPanelViewModelTest
{
    [Test]
    public async Task Open_ReplacingAnEditedChart_ResetsAllState()
    {
        var vm = NewViewModel();

        vm.Open("C:/charts/B/manifest.epk", RangeChartWithHidden());
        vm.Name = "edited";
        vm.SearchKeywords.Add("edited");
        vm.Maps[0].Rating = "999";
        vm.Maps[0].Charters.Add("edited");

        vm.Open("C:/charts/A/manifest.epk", SingleChartWithoutHidden());

        using var _ = Assert.Multiple();
        await Assert.That(vm.Name).IsEqualTo("Song A");
        await Assert.That(vm.Author).IsEqualTo("Author A");
        await Assert.That(vm.SafeForStreamer).IsTrue();
        await Assert.That(vm.IsBpmRange).IsFalse();
        await Assert.That(vm.Bpm).IsEqualTo(120);
        await Assert.That(vm.BpmMin).IsNull();
        await Assert.That(vm.BpmMax).IsNull();
        await Assert.That(vm.SearchKeywords.Count).IsEqualTo(2);
        await Assert.That(vm.SearchKeywords[0]).IsEqualTo("a");
        await Assert.That(vm.SearchKeywords[1]).IsEqualTo("b");
        await Assert.That(vm.HasHiddenDifficulty).IsFalse();
        await Assert.That(vm.Maps.Count).IsEqualTo(2);
        await Assert.That(vm.Maps[0].Difficulty).IsEqualTo(ChartDifficulty.Easy);
        await Assert.That(vm.Maps[1].Difficulty).IsEqualTo(ChartDifficulty.Hard);
        await Assert.That(vm.Maps[0].Rating).IsEqualTo("3");
        await Assert.That(vm.Maps[0].Charters.Count).IsEqualTo(1);
        await Assert.That(vm.Maps[0].Charters[0]).IsEqualTo("alice");
        await Assert.That(vm.Files.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Open_RangeChartWithHidden_PopulatesRangeAndHideSection()
    {
        var vm = NewViewModel();

        vm.Open("C:/charts/B/manifest.epk", RangeChartWithHidden());

        using var _ = Assert.Multiple();
        await Assert.That(vm.IsBpmRange).IsTrue();
        await Assert.That(vm.BpmMin).IsEqualTo(130);
        await Assert.That(vm.BpmMax).IsEqualTo(160);
        await Assert.That(vm.HasHiddenDifficulty).IsTrue();
        await Assert.That(vm.Maps[^1].Difficulty).IsEqualTo(ChartDifficulty.Hidden);
    }

    [Test]
    public async Task CreateNew_FolderContainsChartAndTemporaryFiles_SeedsFilesExcludingManifestAndTemp()
    {
        var folder = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["map1.bms"] = 10,
            ["map2.bms"] = 20,
            ["music.ogg"] = 30,
            ["scratch.tmp"] = 5,
            [ChartFiles.ManifestFileName] = 99
        };
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(folder);

        var vm = new EpkEditorPanelViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            FileSystemService = fileSystem,
            MessageBoxService = IMessageBoxService.Mock(),
            MessagePackSerialization = IMessagePackSerializationService.Mock(),
            NotificationService = INotificationService.Mock()
        };

        vm.CreateNew("C:/charts/New");

        using var _ = Assert.Multiple();
        await Assert.That(vm.Name).IsEqualTo(string.Empty);
        await Assert.That(vm.Author).IsEqualTo(string.Empty);
        await Assert.That(vm.Scene).IsEqualTo("scene_01");
        await Assert.That(vm.IsDirty).IsFalse();
        await Assert.That(vm.Files.Count).IsEqualTo(3);
        await Assert.That(vm.Files.Any(file => file.Name == ChartFiles.ManifestFileName)).IsFalse();
        await Assert.That(vm.Maps.Count).IsEqualTo(2);
        await Assert.That(vm.Maps[0].Difficulty).IsEqualTo(ChartDifficulty.Easy);
        await Assert.That(vm.Maps[1].Difficulty).IsEqualTo(ChartDifficulty.Hard);
    }

    [Test]
    public async Task Save_WhenWriteSucceeds_RaisesSavedWithChartFolder()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase));
        fileSystem.TryWriteFileAtomicAsync(Any<string>(), Any<ReadOnlyMemory<byte>>(), Any<CancellationToken>()).Returns(true);

        var serialization = IMessagePackSerializationService.Mock();
        serialization.SerializeManifest(Any<Manifest>()).Returns([1, 2, 3]);

        var vm = new EpkEditorPanelViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            FileSystemService = fileSystem,
            MessageBoxService = IMessageBoxService.Mock(),
            MessagePackSerialization = serialization,
            NotificationService = INotificationService.Mock()
        };
        vm.Open("C:/charts/A/manifest.epk", SingleChartWithoutHidden());

        string? savedFolder = null;
        vm.Saved += folder => savedFolder = folder;

        await vm.SaveCommand.ExecuteAsync(null);

        await Assert.That(savedFolder).IsEqualTo(Path.GetDirectoryName("C:/charts/A/manifest.epk"));
    }

    [Test]
    public async Task RefreshFiles_WhenABmsAppears_AddsAnEditableMapRowAndKeepsEdits()
    {
        var folder = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["map1.bms"] = 1,
            ["music.ogg"] = 1
        };
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(folder);

        var vm = new EpkEditorPanelViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            FileSystemService = fileSystem,
            MessageBoxService = IMessageBoxService.Mock(),
            MessagePackSerialization = IMessagePackSerializationService.Mock(),
            NotificationService = INotificationService.Mock()
        };
        vm.Open("C:/charts/A/manifest.epk", EasyOnlyChart());
        vm.Maps[0].Rating = "5";

        await Assert.That(vm.Maps.Count).IsEqualTo(1);
        await Assert.That(vm.HasHiddenDifficulty).IsFalse();

        folder["map4.bms"] = 1;
        vm.RefreshFilesCommand.Execute(null);

        using var _ = Assert.Multiple();
        await Assert.That(vm.Maps.Count).IsEqualTo(2);
        await Assert.That(vm.Maps[0].Difficulty).IsEqualTo(ChartDifficulty.Easy);
        await Assert.That(vm.Maps[0].Rating).IsEqualTo("5");
        await Assert.That(vm.Maps[^1].Difficulty).IsEqualTo(ChartDifficulty.Hidden);
        await Assert.That(vm.Maps[^1].Rating).IsEqualTo(string.Empty);
        await Assert.That(vm.HasHiddenDifficulty).IsTrue();
    }

    [Test]
    public async Task RefreshFiles_WhenABmsIsRemoved_DropsTheMapRowAndBlocksSave()
    {
        var folder = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["map1.bms"] = 1,
            ["map2.bms"] = 1,
            ["music.ogg"] = 1
        };
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(folder);

        var vm = new EpkEditorPanelViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            FileSystemService = fileSystem,
            MessageBoxService = IMessageBoxService.Mock(),
            MessagePackSerialization = IMessagePackSerializationService.Mock(),
            NotificationService = INotificationService.Mock()
        };
        vm.Open("C:/charts/A/manifest.epk", SingleChartWithoutHidden());

        await Assert.That(vm.Maps.Count).IsEqualTo(2);
        await Assert.That(vm.CanSave).IsTrue();

        folder.Remove("map2.bms");
        vm.RefreshFilesCommand.Execute(null);

        using var _ = Assert.Multiple();
        await Assert.That(vm.Maps.Count).IsEqualTo(1);
        await Assert.That(vm.Maps[0].Difficulty).IsEqualTo(ChartDifficulty.Easy);
        await Assert.That(vm.CanSave).IsFalse();
    }

    [Test]
    public async Task CanSave_SceneOrMapFieldsCleared_RequiresSceneAndEveryPresentMapFilled()
    {
        var vm = NewViewModel();
        vm.Open("C:/charts/A/manifest.epk", SingleChartWithoutHidden());

        await Assert.That(vm.CanSave).IsTrue();

        vm.Scene = "   ";
        await Assert.That(vm.CanSave).IsFalse();
        vm.Scene = "scene_01";
        await Assert.That(vm.CanSave).IsTrue();

        vm.Maps[1].Charters.Clear();
        await Assert.That(vm.CanSave).IsFalse();
    }

    [Test]
    public async Task CanSave_Map2Missing_ReturnsFalse()
    {
        var vm = NewViewModel();
        vm.Open("C:/charts/A/manifest.epk", EasyOnlyChart());

        await Assert.That(vm.CanSave).IsFalse();
    }

    private static Manifest EasyOnlyChart() => new()
    {
        Schema = Manifest.CurrentSchema,
        Meta = new ManifestMeta
        {
            Name = "Solo",
            Author = "Author",
            Scene = "scene",
            Maps = new Dictionary<string, ManifestMap>(StringComparer.OrdinalIgnoreCase)
            {
                ["map1"] = new() { Rating = "3", Charters = ["alice"] }
            }
        },
        Files = new Dictionary<string, ManifestFileEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["map1.bms"] = new() { Version = 1, Size = 10 },
            ["music.ogg"] = new() { Version = 1, Size = 20 }
        }
    };

    private static EpkEditorPanelViewModel NewViewModel()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["map1.bms"] = 1,
            ["map2.bms"] = 1,
            ["map4.bms"] = 1,
            ["music.ogg"] = 1
        });

        return new EpkEditorPanelViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            FileSystemService = fileSystem,
            MessageBoxService = IMessageBoxService.Mock(),
            MessagePackSerialization = IMessagePackSerializationService.Mock(),
            NotificationService = INotificationService.Mock()
        };
    }

    private static Manifest SingleChartWithoutHidden() => new()
    {
        Schema = Manifest.CurrentSchema,
        Meta = new ManifestMeta
        {
            Name = "Song A",
            Author = "Author A",
            Scene = "scene-a",
            SafeForStreamer = true,
            Bpm = 120,
            SearchKeywords = ["a", "b"],
            Maps = new Dictionary<string, ManifestMap>(StringComparer.OrdinalIgnoreCase)
            {
                ["map1"] = new() { Rating = "3", Charters = ["alice"] },
                ["map2"] = new() { Rating = "6", Charters = ["bob"] }
            }
        },
        Files = new Dictionary<string, ManifestFileEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["map1.bms"] = new() { Version = 1, Size = 10 },
            ["map2.bms"] = new() { Version = 1, Size = 30 },
            ["music.ogg"] = new() { Version = 1, Size = 20 }
        }
    };

    private static Manifest RangeChartWithHidden() => new()
    {
        Schema = Manifest.CurrentSchema,
        Meta = new ManifestMeta
        {
            Name = "Song B",
            Author = "Author B",
            Scene = "scene-b",
            SafeForStreamer = false,
            Bpm = 140,
            BpmMin = 130,
            BpmMax = 160,
            Maps = new Dictionary<string, ManifestMap>(StringComparer.OrdinalIgnoreCase)
            {
                ["map1"] = new() { Rating = "5", Charters = ["carol"] },
                ["map4"] = new() { Rating = "?", Charters = ["dave"] }
            }
        },
        Files = new Dictionary<string, ManifestFileEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["map1.bms"] = new() { Version = 1, Size = 11 },
            ["map4.bms"] = new() { Version = 1, Size = 22 }
        }
    };
}
