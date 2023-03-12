namespace MuseDashModToolsUI.Models;

public class Settings
{
    public string? MuseDashFolder { get; set; }
    public bool AskInstallMuseDashModTools { get; set; } = true;

    public Settings()
    {
        
    }

    public Settings(string? museDashFolder)
    {
        MuseDashFolder = museDashFolder;
    }
}