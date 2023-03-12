using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _client;
    private readonly IDialogueService _dialogueService;

    private const string BaseLink = "MDModsDev/ModLinks/dev/";
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases/latest";

    private const string PrimaryLink = "https://raw.githubusercontent.com/";
    private const string SecondaryLink = "https://raw.fastgit.org/";

    public GitHubService(HttpClient client, IDialogueService dialogueService)
    {
        _client = client;
        _dialogueService = dialogueService;
    }
    public async Task<List<Mod>> GetModsAsync()
    {
        List<Mod> mods;
        try
        {
            mods = (await _client.GetFromJsonAsync<List<Mod>>(
                PrimaryLink + BaseLink +
                "ModLinks.json"))!;
        }
        catch (Exception)
        {
            mods = (await _client.GetFromJsonAsync<List<Mod>>(
                SecondaryLink + BaseLink +
                "ModLinks.json"))!;
        }

        return mods;

    }

    public async Task DownloadModAsync(string link, string path)
    {
        HttpResponseMessage result;
        try
        {
            result = await _client.GetAsync(PrimaryLink + BaseLink + link);
        }
        catch (Exception)
        {
            result = await _client.GetAsync(SecondaryLink + BaseLink + link);
        }
        await using var fs = new FileStream(path, FileMode.CreateNew);
        await result.Content.CopyToAsync(fs);
    }

    public async Task DownloadMelonLoader(string link, string path)
    {
        byte[] result;
        try
        {
            result = await _client.GetByteArrayAsync(PrimaryLink + BaseLink + link);
        }
        catch (Exception)
        {
            result = await _client.GetByteArrayAsync(SecondaryLink + BaseLink + link);
        }

        await File.WriteAllBytesAsync(path, result);
    }

    public async void CheckUpdates()
    {
        _client.DefaultRequestHeaders.Add("User-Agent", "MuseDashModToolsUI");

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        try
        {
            var result = await _client.GetStringAsync(ReleaseInfoLink);
            var doc = JsonDocument.Parse(result);

            if (!doc.RootElement.TryGetProperty("tag_name", out var tagName))
                return;

            var tag = tagName.GetString();
            if (tag == null) return;
            if (!Version.TryParse(tag, out var version)) return;
            if (version <= currentVersion) return;

            string? link;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                link = doc.RootElement.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                link = doc.RootElement.GetProperty("assets")[1].GetProperty("browser_download_url").GetString();
            else
                link = null;

            var parentPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent!.FullName;
            await DownloadUpdates(link!, Directory.GetCurrentDirectory() + ".zip", parentPath);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox("Checking updates failed");
        }
    }

    public async Task DownloadUpdates(string link, string zipPath, string parentPath)
    {
        byte[] result;
        try
        {
            result = await _client.GetByteArrayAsync(link);
        }
        catch (Exception)
        {
            result = await _client.GetByteArrayAsync(link.Replace("github.com", "download.fastgit.org"));
        }

        await File.WriteAllBytesAsync(zipPath, result);
        var fastZip = new FastZip();
        try
        {
            fastZip.ExtractZip(zipPath, parentPath, FastZip.Overwrite.Always, null, null, null, true);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Unable to unzip the latest version of app in\n{zipPath}\nMaybe try manually unzip?");
        }

        try
        {
            File.Delete(zipPath);
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox($"Failed to delete zip file in\n{zipPath}Try manually delete");
        }
    }
}