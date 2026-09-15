namespace Euterpe.Tests.Models.Playback;

[Category("PlaybackStateTests")]
[TestSubject(typeof(PlaybackState))]
public sealed class PlaybackStateTest
{
    [Test]
    public async Task Set_Playing_SetsStatusAndKey()
    {
        var state = new PlaybackState();

        state.Set(PlaybackStatus.Playing, "song");

        using var _ = Assert.Multiple();
        await Assert.That(state.Status).IsEqualTo(PlaybackStatus.Playing);
        await Assert.That(state.PlayingKey).IsEqualTo("song");
    }

    [Test]
    public async Task Set_PauseThenResume_KeepsKeyAndUpdatesStatus()
    {
        var state = new PlaybackState();
        state.Set(PlaybackStatus.Playing, "song");
        state.Set(PlaybackStatus.Paused, "song");

        await Assert.That(state.Status).IsEqualTo(PlaybackStatus.Paused);

        state.Set(PlaybackStatus.Playing, "song");

        using var _ = Assert.Multiple();
        await Assert.That(state.Status).IsEqualTo(PlaybackStatus.Playing);
        await Assert.That(state.PlayingKey).IsEqualTo("song");
    }

    [Test]
    public async Task Set_Idle_ClearsKey()
    {
        var state = new PlaybackState();
        state.Set(PlaybackStatus.Playing, "song");

        state.Set(PlaybackStatus.Idle, null);

        using var _ = Assert.Multiple();
        await Assert.That(state.Status).IsEqualTo(PlaybackStatus.Idle);
        await Assert.That(state.PlayingKey).IsNull();
    }

    [Test]
    public async Task Set_StatusAndPlayingKeyChanged_RaisesPropertyChangedForBoth()
    {
        var state = new PlaybackState();
        var changed = new List<string?>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.Set(PlaybackStatus.Playing, "song");

        using var _ = Assert.Multiple();
        await Assert.That(changed).Contains(nameof(PlaybackState.Status));
        await Assert.That(changed).Contains(nameof(PlaybackState.PlayingKey));
    }
}
