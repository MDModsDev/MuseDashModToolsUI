using System.Collections.Generic;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    List<string> GetModFiles(string path);
    Mod? LoadMod(string filePath);
}