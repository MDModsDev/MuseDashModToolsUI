using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class LocalService : ILocalService
{
    public List<string> GetModFiles(string path) => Directory.GetFiles(path)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll")
        .ToList();
    public Mod? LoadMod(string filePath)
    {
        var mod = new Mod
        {
            IsDisabled = filePath.EndsWith(".disabled"),
        };

        mod.FileName = mod.IsDisabled ? Path.GetFileName(filePath)[..^9] : Path.GetFileName(filePath);
        var assembly = Assembly.Load(File.ReadAllBytes(filePath));
        var attribute = MelonUtils.PullAttributeFromAssembly<MelonInfoAttribute>(assembly);

            mod.Name = attribute.Name;
            mod.LocalVersion = attribute.Version;

            if (mod.Name == null || mod.LocalVersion == null)
            {
                return null;
            }

            mod.Author = attribute.Author;
            mod.HomePage = attribute.DownloadLink;
            mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(filePath);

            return mod;
    }
}