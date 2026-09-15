using Euterpe.Models.VDFs;

namespace Euterpe.Tests.Models.VDFs;

[Category("AppStateTests")]
[TestSubject(typeof(AppState))]
public sealed class AppStateTest
{
    [Test]
    public async Task Constructor_DefaultValues_InitializesZeroEmptyStringsAndEmptyDictionary()
    {
        var state = new AppState();

        using var _ = Assert.Multiple();
        await Assert.That(state.Appid).IsEqualTo(0);
        await Assert.That(state.Name).IsEqualTo(string.Empty);
        await Assert.That(state.InstalledDepots).IsEmpty();
    }
}
