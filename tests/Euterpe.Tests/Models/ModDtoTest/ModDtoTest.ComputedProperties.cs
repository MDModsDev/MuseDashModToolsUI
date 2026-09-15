using Euterpe.Contracts.Mods;
using Euterpe.Localization;

namespace Euterpe.Tests.Models;

public sealed partial class ModDtoTest
{
    [Test]
    [Arguments(true, "MyMod.disabled", "MyMod.dll")]
    [Arguments(false, "MyMod.dll", "MyMod.disabled")]
    public async Task LocalAndReversedFileName_DisabledFlagVariations_UseMatchingExtensions(bool isDisabled, string expectedLocal, string expectedReversed)
    {
        var mod = Create(localFnWithoutExt: "MyMod", disabled: isDisabled);

        using var _ = Assert.Multiple();
        await Assert.That(mod.LocalFileName).IsEqualTo(expectedLocal);
        await Assert.That(mod.ReversedFileName).IsEqualTo(expectedReversed);
    }

    [Test]
    [Arguments(null, false)]
    [Arguments("MyMod", true)]
    public async Task IsLocal_LocalFileNameVariations_ReflectsFileNamePresence(string? fileNameWithoutExt, bool expected)
    {
        var mod = Create(localFnWithoutExt: fileNameWithoutExt);
        await Assert.That(mod.IsLocal).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", false)]
    [Arguments("MyMod.dll", true)]
    public async Task HasDownloadSource_FileNameVariations_ReflectsFileNamePresence(string fileName, bool expected)
    {
        var mod = Create(fileName);
        await Assert.That(mod.HasDownloadSource).IsEqualTo(expected);
    }

    public static IEnumerable<Func<(bool isLocal, bool hasDownload, ModState state, bool expected)>> InstallableCases()
    {
        yield return () => (false, true, ModState.Normal, true);
        yield return () => (false, true, ModState.Outdated, true);
        yield return () => (true, true, ModState.Normal, false);
        yield return () => (false, false, ModState.Normal, false);
        yield return () => (false, true, ModState.Incompatible, false);
    }

    [Test]
    [MethodDataSource(nameof(InstallableCases))]
    public async Task IsInstallable_LocalDownloadAndStateCombinations_ReturnsExpectedAvailability((bool isLocal, bool hasDownload, ModState state, bool expected) data)
    {
        var mod = Create(
            data.hasDownload ? "MyMod.dll" : "",
            data.isLocal ? "MyMod" : null);
        mod.State = data.state;

        await Assert.That(mod.IsInstallable).IsEqualTo(data.expected);
    }

    [Test]
    [Arguments(true, ModState.Modified, true)]
    [Arguments(true, ModState.Normal, false)]
    [Arguments(true, ModState.Outdated, false)]
    [Arguments(false, ModState.Modified, false)]
    public async Task IsReinstallable_LocalAndStateCombinations_RequiresLocalModifiedMod(bool isLocal, ModState state, bool expected)
    {
        var mod = Create(localFnWithoutExt: isLocal ? "MyMod" : null);
        mod.State = state;
        await Assert.That(mod.IsReinstallable).IsEqualTo(expected);
    }

    [Test]
    public async Task ReasonDisplay_Duplicated_ListsDuplicateFiles()
    {
        var mod = Create();
        mod.DuplicatedModPaths = ["MyMod.dll", "MyMod.disabled"];
        mod.State = ModState.Duplicated;

        await Assert.That(mod.ReasonDisplay).IsEqualTo(string.Format(
            XAML.ModState_Duplicated_Reason,
            "MyMod.dll, MyMod.disabled"));
    }

    [Test]
    public async Task ReasonDisplay_Incompatible_PreservesIncompatibleReason()
    {
        var mod = Create();
        mod.GameVersion = "5.0.0";
        mod.IncompatibleReason = ModIncompatibleReason.GameVersion;
        mod.State = ModState.Incompatible;

        await Assert.That(mod.ReasonDisplay).IsEqualTo(string.Format(XAML.ModIncompatibleReason_GameVersion, "5.0.0"));
    }

    [Test]
    public async Task DuplicatedModPaths_ValueChanged_RaisesPropertyChangedForReasonDisplay()
    {
        var mod = Create();
        var changedProperties = new List<string?>();
        mod.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        mod.DuplicatedModPaths = ["MyMod.dll", "MyMod.disabled"];

        await Assert.That(changedProperties).Contains(nameof(ModDto.ReasonDisplay));
    }

    [Test]
    public async Task HasDependency_DependencyCollectionsChange_ReflectsWhetherAnyExist()
    {
        var mod = Create();

        using var _ = Assert.Multiple();
        await Assert.That(mod.HasDependency).IsFalse();

        mod.ModDependencies = ["DepA"];
        await Assert.That(mod.HasDependency).IsTrue();

        mod.ModDependencies = [];
        mod.LibDependencies = ["LibA"];
        await Assert.That(mod.HasDependency).IsTrue();
    }

    [Test]
    public async Task DependencyNames_ModAndLibDependencies_ReturnsCombinedNames()
    {
        var mod = Create();
        mod.ModDependencies = ["ModA", "ModB"];
        mod.LibDependencies = ["LibX"];

        await Assert.That(mod.DependencyNames).IsEquivalentTo(["ModA", "ModB", "LibX"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task DependencyNames_NoDependencies_ReturnsEmpty()
    {
        var mod = Create();
        await Assert.That(mod.DependencyNames).IsEmpty();
    }

    [Test]
    public async Task HasScreenshots_ScreenshotsAdded_ReturnsTrue()
    {
        var mod = Create();

        using var _ = Assert.Multiple();
        await Assert.That(mod.HasScreenshots).IsFalse();

        mod.Screenshots = [new ModScreenshot { Url = "https://example.com/img.png" }];
        await Assert.That(mod.HasScreenshots).IsTrue();
    }
}
