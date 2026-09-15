using Euterpe.Features.Setting;

namespace Euterpe.Tests.App.ViewModels;

[Category("DownloadPanelViewModelTests")]
[TestSubject(typeof(DownloadPanelViewModel))]
public sealed class DownloadPanelViewModelTest
{
    [Test]
    public async Task UpdateChannels_DefaultOptions_ContainsEveryChannelInDeclarationOrder()
    {
        await Assert.That(DownloadPanelViewModel.UpdateChannels.Select(static option => option.Value))
            .IsEquivalentTo(
                [UpdateChannel.Stable, UpdateChannel.Beta],
                EqualityComparer<UpdateChannel>.Default,
                CollectionOrdering.Matching);
    }
}
