using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Views;
using Splat;

namespace MuseDashModToolsUI;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DataContext = GetRequiredService<IMainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = DataContext
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    private static T GetRequiredService<T>() => Locator.Current.GetRequiredService<T>();
}