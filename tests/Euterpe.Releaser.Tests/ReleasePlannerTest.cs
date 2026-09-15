using Euterpe.Releaser;
using Semver;

namespace Euterpe.Releaser.Tests;

[Category("ReleaserTests")]
[TestSubject(typeof(ReleasePlanner))]
public sealed class ReleasePlannerTest
{
    private static readonly ReleaseRuntime Runtime = ReleaseRuntime.Parse("win-x64");

    [Test]
    public async Task GetPackageChannels_Prerelease_ReturnsBetaChannel()
    {
        var channels = ReleasePlanner.GetPackageChannels(
            Runtime,
            SemVersion.Parse("2.2.0-beta.1", SemVersionStyles.Strict),
            false);

        await Assert.That(channels.Count).IsEqualTo(1);
        await Assert.That(channels[0]).IsEqualTo("win-x64-beta");
    }

    [Test]
    public async Task GetPackageChannels_StableWithoutBetaBase_ReturnsStableChannel()
    {
        var channels = ReleasePlanner.GetPackageChannels(
            Runtime,
            SemVersion.Parse("2.2.0", SemVersionStyles.Strict),
            false);

        await Assert.That(channels.Count).IsEqualTo(1);
        await Assert.That(channels[0]).IsEqualTo("win-x64-stable");
    }

    [Test]
    public async Task GetPackageChannels_StableWithBetaBase_ReturnsStableAndBetaChannels()
    {
        var channels = ReleasePlanner.GetPackageChannels(
            Runtime,
            SemVersion.Parse("2.2.0", SemVersionStyles.Strict),
            true);

        await Assert.That(channels.Count).IsEqualTo(2);
        await Assert.That(channels[0]).IsEqualTo("win-x64-stable");
        await Assert.That(channels[1]).IsEqualTo("win-x64-beta");
    }
}
