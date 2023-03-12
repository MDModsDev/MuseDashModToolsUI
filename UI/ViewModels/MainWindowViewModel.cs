using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DynamicData;
using DynamicData.Binding;
using ICSharpCode.SharpZipLib.Zip;
using MelonLoader;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using ReactiveUI;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    public ReactiveCommand<Unit, Unit> FilterAllCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterInstalledCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterEnabledCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterOutdatedCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterIncompatibleCommand { get; }
    public ReactiveCommand<Mod, Unit> SelectedItemCommand { get; }
    public ReactiveCommand<Mod, Unit> InstallModCommand { get; }
    public ReactiveCommand<Mod, Unit> ReinstallModCommand { get; }
    public ReactiveCommand<Mod, Unit> RemoveModCommand { get; }
    public ReactiveCommand<Mod, Unit> ToggleModCommand { get; }
    public ReactiveCommand<string, Unit> OpenUrlCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenFolderDialogueCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenModsFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> InstallMelonLoaderCommand { get; }
    public ReactiveCommand<Unit, Unit> UninstallMelonLoaderCommand { get; }

    private Mod _selectedItem;

    public Mod SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    private string _filter;

    public string Filter
    {
        get => _filter;
        set
        {
            this.RaiseAndSetIfChanged(ref _filter, value);
            _sourceCache.Refresh();
        }
    }

    private Filter _categoryFilter;

    public Filter CategoryFilter
    {
        get => _categoryFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _categoryFilter, value);
            _sourceCache.Refresh();
        }
    }

    private readonly SourceCache<Mod, string> _sourceCache = new(x => x.Name!);
    private readonly ReadOnlyObservableCollection<Mod> _mods;
    public ReadOnlyObservableCollection<Mod> Mods => _mods;
    private Settings _settings = new();
    private bool _isValidPath;
    private string _currentGameVersion;
    private string ModsFolder => !string.IsNullOrEmpty(_settings.MuseDashFolder) ? Path.Join(_settings.MuseDashFolder, "Mods") : string.Empty;

    private readonly IGitHubService _gitHubService;
    private readonly ILocalService _localService;
    private readonly IDialogueService _dialogueService;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(IGitHubService gitHubService, ILocalService localService, IDialogueService dialogueService)
    {
        _gitHubService = gitHubService;
        _localService = localService;
        _dialogueService = dialogueService;

        FilterAllCommand = ReactiveCommand.Create(OnFilterAll);
        FilterInstalledCommand = ReactiveCommand.Create(OnFilterInstalled);
        FilterEnabledCommand = ReactiveCommand.Create(OnFilterEnabled);
        FilterOutdatedCommand = ReactiveCommand.Create(OnFilterOutdated);
        FilterIncompatibleCommand = ReactiveCommand.Create(OnFilterIncompatible);

        OpenUrlCommand = ReactiveCommand.Create<string>(OpenUrl);
        OpenFolderDialogueCommand = ReactiveCommand.CreateFromTask(OnChoosePath);
        OpenModsFolderCommand = ReactiveCommand.CreateFromTask(OpenModsFolder);
        InstallMelonLoaderCommand = ReactiveCommand.CreateFromTask(OnInstallMelonLoader);
        UninstallMelonLoaderCommand = ReactiveCommand.CreateFromTask(OnUninstallMelonLoader);

        SelectedItemCommand = ReactiveCommand.Create<Mod>(OnSelectedItem);
        InstallModCommand = ReactiveCommand.CreateFromTask<Mod>(OnInstallMod);
        ReinstallModCommand = ReactiveCommand.CreateFromTask<Mod>(OnReinstallMod);
        RemoveModCommand = ReactiveCommand.CreateFromTask<Mod>(OnDeleteMod);
        ToggleModCommand = ReactiveCommand.CreateFromTask<Mod>(OnToggleMod);

        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(Filter) || x.Name!.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => CategoryFilter != Models.Filter.Enabled || (CategoryFilter == Models.Filter.Enabled && x is { IsDisabled: false, IsLocal: true }))
            .Filter(x => CategoryFilter != Models.Filter.Outdated || (CategoryFilter == Models.Filter.Outdated && x.State == UpdateState.Outdated))
            .Filter(x => CategoryFilter != Models.Filter.Installed || (CategoryFilter == Models.Filter.Installed && x.IsLocal))
            .Filter(x => CategoryFilter != Models.Filter.Incompatible || (CategoryFilter == Models.Filter.Incompatible && x is { IsIncompatible: true, IsLocal: true }))
            .Sort(SortExpressionComparer<Mod>.Ascending(t => t.Name!))
            .Bind(out _mods)
            .Subscribe();

        RxApp.MainThreadScheduler.Schedule(InitializeSettings);
        //RxApp.MainThreadScheduler.Schedule(_gitHubService.CheckUpdates);
        AppDomain.CurrentDomain.ProcessExit += OnExit!;
    }

    private async void InitializeSettings()
    {
        try
        {
            if (!File.Exists("appsettings.json"))
            {
                await _dialogueService.CreateErrorMessageBox("Warning", "You haven't choose Muse Dash Folder\nPlease choose the folder");
                await OnChoosePath();
                Debug.WriteLine("run");
                return;
            }

            var text = await File.ReadAllTextAsync("appsettings.json");
            if (string.IsNullOrEmpty(text))
            {
                await _dialogueService.CreateErrorMessageBox("Warning", "Your stored Muse Dash Folder path is null\nPlease choose the correct folder");
                await OnChoosePath();
                return;
            }

            _settings = JsonSerializer.Deserialize<Settings>(text)!;
            InitializeModList();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task<bool> CheckValidPath()
    {
        var exePath = Path.Join(_settings.MuseDashFolder, "MuseDash.exe");
        var gameAssemblyPath = Path.Join(_settings.MuseDashFolder, "GameAssembly.dll");
        var userDataPath = Path.Join(_settings.MuseDashFolder, "UserData");
        if (!File.Exists(exePath) || !File.Exists(gameAssemblyPath))
        {
            await _dialogueService.CreateErrorMessageBox("Couldn't find MuseDash.exe or GameAssembly.dll\nPlease choose the right folder");
            await OnChoosePath();
            return false;
        }

        try
        {
            var version = FileVersionInfo.GetVersionInfo(exePath).FileVersion;
            if (version is not "2019.4.32.16288752")
            {
                await _dialogueService.CreateErrorMessageBox("Muse Dash.exe is not correct version \nAre you using a pirated or modified version?");
                return false;
            }

            if (!Directory.Exists(ModsFolder))
                Directory.CreateDirectory(ModsFolder);

            if (!Directory.Exists(userDataPath))
                Directory.CreateDirectory(userDataPath);

            var cfgFilePath = Path.Join(_settings.MuseDashFolder, "UserData", "MuseDashModTools.cfg");
            if (!File.Exists(cfgFilePath))
                await File.WriteAllTextAsync(cfgFilePath, Process.GetCurrentProcess().MainModule!.FileName);
            else
            {
                var path = await File.ReadAllTextAsync(cfgFilePath);
                if (path != Process.GetCurrentProcess().MainModule!.FileName)
                    await File.WriteAllTextAsync(cfgFilePath, Process.GetCurrentProcess().MainModule!.FileName);
            }

            await ReadGameVersion();

            return true;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox("Failed to verify MuseDash.exe\nMake sure you selected the right folder");
            return false;
        }
    }

    private async Task ReadGameVersion()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(_settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadIncludedClassPackage();
            if (!instance.file.Metadata.TypeTreeEnabled)
                assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];

            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)?.Get("bundleVersion");
            _currentGameVersion = bundleVersion!.AsString;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Cannot read current game version\nDo you fully installed Muse Dash?\nPlease check your globalgamemanagers file in\n{bundlePath}");
            Environment.Exit(0);
        }
    }

    private async Task CheckMelonLoaderInstall()
    {
        var melonLoaderFolder = Path.Join(_settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile)) return;
        var install = await _dialogueService.CreateConfirmMessageBox("Notice", "You did not install MelonLoader\nWhich is needed to run all the mods\nInstall Now?");
        if (install)
            await OnInstallMelonLoader();
    }

    private async void InitializeModList()
    {
        _isValidPath = await CheckValidPath();
        if (!_isValidPath) return;
        await CheckMelonLoaderInstall();

        var webMods = await _gitHubService.GetModsAsync();
        var localPaths = _localService.GetModFiles(ModsFolder);
        List<Mod>? localMods;
        try
        {
            localMods = localPaths.Select(_localService.LoadMod).Where(mod => mod is not null).ToList()!;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox("Your downloaded mods are broken\nPlease delete 0kb mod if it exist");
            Environment.Exit(0);
            return;
        }

        var isTracked = new bool[localMods.Count];
        foreach (var webMod in webMods)
        {
            var localMod = localMods.FirstOrDefault(x => x.Name == webMod.Name);
            var localModIdx = localMods.IndexOf(localMod!);

            if (localMod is null)
            {
                webMod.IsTracked = true;
                webMod.IsIncompatible = !CheckCompatible(webMod);
                _sourceCache.AddOrUpdate(webMod);
                if (!_settings.AskInstallMuseDashModTools)
                    continue;
                await CheckModToolsInstall(webMod);
                continue;
            }

            if (localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
            }

            isTracked[localModIdx] = true;
            localMod.IsTracked = true;
            localMod.Version = webMod.Version;
            localMod.GameVersion = webMod.GameVersion;
            localMod.DependentLibs = webMod.DependentLibs;
            localMod.DependentMods = webMod.DependentMods;
            localMod.IncompatibleMods = webMod.IncompatibleMods;
            localMod.DownloadLink = webMod.DownloadLink;
            localMod.HomePage = webMod.HomePage;

            var versionDate = new Version(webMod.Version!) > new Version(localMod.LocalVersion!) ? -1 : new Version(webMod.Version!) < new Version(localMod.LocalVersion!) ? 1 : 0;
            localMod.State = (UpdateState)versionDate;
            localMod.IsShaMismatched = versionDate == 0 && webMod.SHA256 != localMod.SHA256;
            if (localMod.IsShaMismatched)
                localMod.State = UpdateState.Modified;
            localMod.IsIncompatible = !CheckCompatible(localMod);
            _sourceCache.AddOrUpdate(localMod);
        }

        for (var i = 0; i < isTracked.Length; i++)
        {
            if (!isTracked[i] && localMods.Count(x => x.Name == localMods[i]!.Name) == 1)
            {
                _sourceCache.AddOrUpdate(localMods[i]!);
            }
        }
    }

    private bool CheckCompatible(Mod mod) => mod.CompatibleGameVersion == "All" || mod.GameVersion!.Contains(_currentGameVersion);

    private async Task CheckModToolsInstall(Mod mod)
    {
        if (mod.Name != "MuseDashModTools") return;
        var result = await _dialogueService.CreateCustomConfirmMessageBox("You don't have MuseDashModTools mod installed\nWhich checks available update for all the mods when launching Muse Dash\nInstall Now?");
        switch (result)
        {
            case "Yes":
                await OnInstallMod(mod);
                break;
            case "No and Don't Ask Again":
                _settings.AskInstallMuseDashModTools = false;
                break;
        }
    }

    private async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            await _dialogueService.CreateErrorMessageBox("This mod does not have an available resource for download.\n");
            return;
        }

        var errors = new StringBuilder();
        var modPaths = new List<string>();
        var mods = new List<Mod>();
        try
        {
            var path = Path.Join(Path.GetTempPath(), item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            if (!File.Exists(path))
            {
                await _gitHubService.DownloadModAsync(item.DownloadLink, path);
            }

            modPaths.Add(path);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case WebException:
                    errors.AppendLine($"Mod installation failed\nAre you online? {ex.ToString()}");
                    break;

                case SecurityException:
                case UnauthorizedAccessException:
                case IOException:
                    errors.AppendLine($"Mod installation failed\nIs the game running? {ex.ToString()}");
                    break;

                default:
                    errors.AppendLine($"Mod installation failed\n{ex.ToString()}");
                    break;
            }
        }

        foreach (var mod in item.DependentMods)
        {
            var dependedMod = Mods.FirstOrDefault(x => x.DownloadLink == mod && x.IsLocal);
            if (dependedMod is not null) continue;
            try
            {
                var path = Path.Join(Path.GetTempPath(), mod.Split("/")[1]);
                if (!File.Exists(path))
                {
                    await _gitHubService.DownloadModAsync(mod, path);
                }

                modPaths.Add(path);
            }
            catch (Exception ex)
            {
                errors.AppendLine($"Dependency failed to install\n {ex.ToString()}");
            }
        }

        if (modPaths.Count > 0)
        {
            foreach (var path in modPaths)
            {
                var fullPath = Path.Join(ModsFolder, Path.GetFileName(path));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                File.Move(path, fullPath);
                var mod = _localService.LoadMod(fullPath);
                if (mod is not null)
                {
                    mods.Add(mod);
                }
            }
        }

        if (errors.Length > 0)
        {
            await _dialogueService.CreateErrorMessageBox(errors.ToString());
            return;
        }

        if (mods.Count > 0)
        {
            foreach (var mod in mods)
            {
                var existedMod = _mods.FirstOrDefault(x => x.Name == mod.Name);
                if (existedMod is not null)
                {
                    _sourceCache.Remove(existedMod);
                }

                _sourceCache.AddOrUpdate(mod);
            }
        }

        await _dialogueService.CreateMessageBox("Success", $"{item.Name} has been successfully installed\n");
    }

    private async Task OnReinstallMod(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            await OnInstallMod(item);
            return;
        }

        var result = await _dialogueService.CreateConfirmMessageBox($"You are asking to reinstall {item.Name}\nPlease confirm your operation");
        if (!result) return;
        await OnInstallMod(item);
    }

    private async Task OnToggleMod(Mod item)
    {
        //Kind of a bummer that I have to reverse the boolean here, due to binding triggering before the command executes. If you find a better way for this, hit me with a big fat PR
        item.IsDisabled = !item.IsDisabled;
        try
        {
            File.Move(
                Path.Join(ModsFolder, item.FileNameExtended()),
                Path.Join(ModsFolder, item.FileNameExtended(true)));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\nUnauthorized");
                    break;

                case IOException:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\nIs the game running?");
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\n");
                    break;
            }

            item.IsDisabled = !item.IsDisabled;
        }

        item.IsDisabled = !item.IsDisabled;
    }

    private async Task OnDeleteMod(Mod item)
    {
        var path = Path.Join(ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            await _dialogueService.CreateErrorMessageBox("Cannot delete file that doesn't exist");
            return;
        }

        try
        {
            File.Delete(path);
            _sourceCache.Remove(item);

            var mods = await _gitHubService.GetModsAsync();
            var webMod = mods.FirstOrDefault(x => x.Name == item.Name);
            if (webMod is not null)
            {
                _sourceCache.AddOrUpdate(webMod);
            }

            await _dialogueService.CreateMessageBox("Success", $"{item.Name} has been successfully deleted.\n");
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                case IOException:
                    await _dialogueService.CreateErrorMessageBox("Mod uninstall failed\nIs the game running?");
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox("Mod uninstall failed");
                    break;
            }
        }
    }

    private async Task OnInstallMelonLoader()
    {
        if (!_isValidPath) return;
        var zipPath = Path.Join(_settings.MuseDashFolder, "MelonLoader.zip");
        if (!File.Exists(zipPath))
        {
            try
            {
                await _gitHubService.DownloadMelonLoader("MelonLoader.zip", zipPath);
            }
            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    await _dialogueService.CreateErrorMessageBox("MelonLoader download failed due to internet\nAre you online?");
                    return;
                }

                await _dialogueService.CreateErrorMessageBox($"MelonLoader download failed\n{ex}");
                return;
            }
        }

        try
        {
            var fastZip = new FastZip();
            fastZip.ExtractZip(zipPath, _settings.MuseDashFolder, FastZip.Overwrite.Always, null, null, null, true);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Cannot unzip MelonLoader.zip in\n{zipPath}\nPlease make sure your game is not running\nThen try manually unzip");
            return;
        }

        try
        {
            File.Delete(zipPath);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Failed to delete MelonLoader.zip in\n{zipPath}\nTry manually delete");
            return;
        }

        await _dialogueService.CreateMessageBox("Success", "MelonLoader has been successfully installed\n");
    }

    private async Task OnUninstallMelonLoader()
    {
        if (!_isValidPath) return;
        var result = await _dialogueService.CreateConfirmMessageBox("You are asking to uninstall MelonLoader\nPlease confirm your operation");
        if (!result) return;
        var melonLoaderFolder = Path.Join(_settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(_settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(melonLoaderFolder))
        {
            try
            {
                Directory.Delete(melonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                await _dialogueService.CreateMessageBox("Success", "MelonLoader has been successfully uninstalled\n");
            }
            catch (Exception)
            {
                await _dialogueService.CreateErrorMessageBox("Cannot uninstall MelonLoader\nPlease make sure your game is not running!");
            }
        }
        else
            await _dialogueService.CreateErrorMessageBox("Cannot find MelonLoader Folder\nHave you installed MelonLoader?");
    }

    private async Task OnChoosePath()
    {
        while (true)
        {
            var dialogue = new OpenFolderDialog { Title = "Choose Muse Dash Folder" };
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var path = await dialogue.ShowAsync(desktop.MainWindow);
                if (string.IsNullOrEmpty(path))
                {
                    await _dialogueService.CreateErrorMessageBox("The path you chose is invalid. Try again...");
                    continue;
                }

                _settings.MuseDashFolder = path;
                var json = JsonSerializer.Serialize(_settings);
                await File.WriteAllTextAsync("appsettings.json", json);
                RxApp.MainThreadScheduler.Schedule(InitializeModList);
            }

            break;
        }
    }

    private void OpenUrl(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private async Task OpenModsFolder()
    {
        if (!_isValidPath)
        {
            await _dialogueService.CreateErrorMessageBox("Choose correct Muse Dash folder first!");
            await OnChoosePath();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = ModsFolder,
            UseShellExecute = true
        });
    }

    private void OnFilterAll() => CategoryFilter = Models.Filter.All;

    private void OnFilterInstalled() => CategoryFilter = Models.Filter.Installed;

    private void OnFilterEnabled() => CategoryFilter = Models.Filter.Enabled;

    private void OnFilterOutdated() => CategoryFilter = Models.Filter.Outdated;

    private void OnFilterIncompatible() => CategoryFilter = Models.Filter.Incompatible;

    private void OnSelectedItem(Mod item)
    {
        item.IsExpanded = !item.IsExpanded;
        SelectedItem = item;
    }

    private void OnExit(object sender, EventArgs e)
    {
        var json = JsonSerializer.Serialize(_settings);
        File.WriteAllText("appsettings.json", json);
    }
}