using Euterpe.Abstractions;
using Euterpe.Features.Setting;
using Euterpe.Models;
using Euterpe.Models.Common;
using Euterpe.Models.Games;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Headless.Tests.Views;

[Category("DownloadPanelTests")]
[TestSubject(typeof(DownloadPanel))]
public sealed class DownloadPanelTest : HeadlessTest
{
    [Test]
    public Task UpdateChannel_SelectionChanges_StaysSynchronizedWithConfig() => RunOnUI(async () =>
    {
        var config = new Config
        {
            MuseDash = new MuseDashConfig(),
            MuseDash2 = new MuseDash2Config(),
            UpdateChannel = UpdateChannel.Beta
        };
        var viewModel = new DownloadPanelViewModel
        {
            Config = config,
            Launcher = IPlatformLauncher.Mock(),
            Logger = NullLogger<DownloadPanelViewModel>.Instance
        };
        var view = new DownloadPanel { DataContext = viewModel };
        var window = new Window { Content = view, Width = 800, Height = 600 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var comboBox = view.GetVisualDescendants().OfType<ComboBox>().Single();

        using var _ = Assert.Multiple();
        await Assert.That(comboBox.SelectedValue).IsEqualTo(UpdateChannel.Beta);

        comboBox.SelectedValue = UpdateChannel.Stable;
        Dispatcher.UIThread.RunJobs();

        await Assert.That(config.UpdateChannel).IsEqualTo(UpdateChannel.Stable);
    });
}
