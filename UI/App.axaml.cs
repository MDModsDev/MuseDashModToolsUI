using Avalonia.Controls.ApplicationLifetimes;
using MuseDashModToolsUI.Views;

namespace MuseDashModToolsUI;

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}