using Avalonia.Labs.AnimatedImage;
using Euterpe.Features.Charting;
using Euterpe.Features.Home;
using Euterpe.Features.Logging;
using Euterpe.Features.Modding;
using Euterpe.Features.Setting;
using Euterpe.Features.Setup;
using Euterpe.Features.Wizard;
using Euterpe.Shell;

namespace Euterpe.Headless.Tests.Views;

[Category("ViewSmokeTests")]
public sealed class ViewSmokeTest : HeadlessTest
{
    [Test]
    public Task Show_ChartingPage_LoadsIntoVisualTree() => Smoke(() => new ChartingPage());

    [Test]
    public Task Show_HomePage_LoadsIntoVisualTree() => RunOnUI(async () =>
    {
        var view = new HomePage();
        var window = new Window { Content = view, Width = 800, Height = 600 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var source = view.GetVisualDescendants().OfType<AnimatedImage>().Single().Source;
        using var _ = Assert.Multiple();
        await Assert.That(view.IsLoaded).IsTrue();
        await Assert.That(source).IsNotNull();
        await Assert.That(source!.IsInitialized).IsTrue();
        await Assert.That(source.FrameCount).IsGreaterThan(1);
    });

    [Test]
    public Task Show_LoggingPage_LoadsIntoVisualTree() => Smoke(() => new LoggingPage());

    [Test]
    public Task Show_ModdingPage_LoadsIntoVisualTree() => Smoke(() => new ModdingPage());

    [Test]
    public Task Show_SettingPage_LoadsIntoVisualTree() => Smoke(() => new SettingPage());

    [Test]
    public Task Show_RepairDialog_LoadsIntoVisualTree() => Smoke(() => new RepairDialog());

    [Test]
    public Task Show_WizardDialog_LoadsIntoVisualTree() => Smoke(() => new WizardDialog());

    [Test]
    public Task Show_ExecutionPage_LoadsIntoVisualTree() => Smoke(() => new ExecutionPage());

    [Test]
    public Task Show_GamePathPage_LoadsIntoVisualTree() => Smoke(() => new GamePathPage());

    [Test]
    public Task Show_RolePage_LoadsIntoVisualTree() => Smoke(() => new RolePage());

    [Test]
    public Task Show_CharterToolkitPanel_LoadsIntoVisualTree() => Smoke(() => new CharterToolkitPanel());

    [Test]
    public Task Show_EpkEditorPanel_LoadsIntoVisualTree() => Smoke(() => new EpkEditorPanel());

    [Test]
    public Task Show_ChartManagePanel_LoadsIntoVisualTree() => Smoke(() => new ChartManagePanel());

    [Test]
    public Task Show_AppLogPanel_LoadsIntoVisualTree() => Smoke(() => new AppLogPanel());

    [Test]
    public Task Show_MelonLoaderLogPanel_LoadsIntoVisualTree() => Smoke(() => new MelonLoaderLogPanel());

    [Test]
    public Task Show_MelonLoaderPanel_LoadsIntoVisualTree() => Smoke(() => new MelonLoaderPanel());

    [Test]
    public Task Show_ModDevelopPanel_LoadsIntoVisualTree() => Smoke(() => new ModDevelopPanel());

    [Test]
    public Task Show_ModManagePanel_LoadsIntoVisualTree() => Smoke(() => new ModManagePanel());

    [Test]
    public Task Show_AboutPanel_LoadsIntoVisualTree() => Smoke(() => new AboutPanel());

    [Test]
    public Task Show_AdvancedPanel_LoadsIntoVisualTree() => Smoke(() => new AdvancedPanel());

    [Test]
    public Task Show_AppearancePanel_LoadsIntoVisualTree() => Smoke(() => new AppearancePanel());

    [Test]
    public Task Show_DownloadPanel_LoadsIntoVisualTree() => Smoke(() => new DownloadPanel());

    [Test]
    public Task Show_ExperiencePanel_LoadsIntoVisualTree() => Smoke(() => new ExperiencePanel());

    [Test]
    public Task Show_FileManagementPanel_LoadsIntoVisualTree() => Smoke(() => new FileManagementPanel());

    [Test]
    public Task Show_MainWindow_LoadsIntoVisualTree() => RunOnUI(async () =>
    {
        var window = new MainWindow();
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(window).IsNotNull();
        await Assert.That(window.IsLoaded).IsTrue();
    });

    [Test]
    public Task Show_MainSplashWindow_LoadsIntoVisualTree() => RunOnUI(async () =>
    {
        var window = new MainSplashWindow { MainWindowFactory = () => new MainWindow() };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(window).IsNotNull();
        await Assert.That(window.IsLoaded).IsTrue();
    });

    [Test]
    public Task Show_CrashWindow_LoadsIntoVisualTree() => RunOnUI(async () =>
    {
        var window = new CrashWindow();
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(window).IsNotNull();
        await Assert.That(window.IsLoaded).IsTrue();
    });

    private static Task Smoke(Func<Control> factory) => RunOnUI(async () =>
    {
        var view = factory();
        var window = new Window { Content = view, Width = 800, Height = 600 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(view).IsNotNull();
        await Assert.That(view.IsLoaded).IsTrue();
    });
}
