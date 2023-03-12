using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace MuseDashModToolsUI.Models;

public class Mod : ReactiveObject
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? LocalVersion { get; set; }
    [JsonIgnore] public UpdateState State { get; set; }
    [JsonIgnore] public bool IsIncompatible { get; set; }
    [JsonIgnore] public bool IsUpdatable => IsLocal && State != UpdateState.Normal;
    public string? Author { get; set; }
    public string? FileName { get; set; }
    [JsonIgnore] public bool IsLocal => FileName is not null;
    private bool _isDisabled;

    [JsonIgnore]
    public bool IsDisabled
    {
        get => _isDisabled;
        set => this.RaiseAndSetIfChanged(ref _isDisabled, value);
    }

    [JsonIgnore] public bool IsTracked { get; set; }
    [JsonIgnore] public bool IsShaMismatched { get; set; }
    [JsonIgnore] public bool IsDuplicated { get; set; }

    [JsonIgnore] public string XamlDescription => $"{Description} \n\nAuthor: {Author} \nVersion: {Version}\nCompatible Game Version: {CompatibleGameVersion}";
    private bool _isExpanded;

    [JsonIgnore]
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public string? DownloadLink { get; set; }
    public string? HomePage { get; set; }

    [JsonIgnore]
    public bool IsValidHomePage => HomePage is not null && Uri.TryCreate(HomePage, UriKind.Absolute, out var uriResult) &&
                                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public string[]? GameVersion { get; set; }

    [JsonIgnore]
    public string CompatibleGameVersion
    {
        get
        {
            if (GameVersion is null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var version in GameVersion)
            {
                if (version == "*")
                    return "All";

                sb.Append(version + ", ");
            }

            return sb.ToString()[..^2];
        }
    }

    public string? Description { get; set; }
    public List<string> DependentMods { get; set; } = new();
    public List<string> DependentLibs { get; set; } = new();
    [JsonIgnore] private int DependencyCount => DependentMods.Count + DependentLibs.Count;

    [JsonIgnore]
    public string? DependencyNames
    {
        get
        {
            if (DependencyCount == 0)
                return null;
            var sb = new StringBuilder();
            foreach (var dependentMod in DependentMods)
            {
                sb.AppendLine(dependentMod[5..]);
            }

            foreach (var dependentLib in DependentLibs)
            {
                sb.AppendLine(dependentLib[5..]);
            }

            return sb.ToString();
        }
    }

    public List<string> IncompatibleMods { get; set; } = new();
    public string? SHA256 { get; set; }
    public string FileNameExtended(bool reverse = false)
    {
        return FileName + ((reverse ? !IsDisabled : IsDisabled) ? ".disabled" : "");
    }
}

public enum UpdateState
{
    Normal = 0,
    Outdated = -1,
    Newer = 1,
    Modified = 2
}

public enum Filter
{
    All = 0,
    Installed = 1,
    Enabled = 2,
    Outdated = 3,
    Incompatible = 4
}