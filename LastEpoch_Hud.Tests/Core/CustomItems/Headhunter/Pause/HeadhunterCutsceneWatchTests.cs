using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterCutsceneWatchTests
{
    private const double Begin = 10;

    private readonly HeadhunterCutsceneWatch _watch = new();

    [Theory]
    [InlineData(69.17, 69.17)]
    [InlineData(0, 30)]
    [InlineData(-1, 30)]
    [InlineData(double.NaN, 30)]
    [InlineData(500, 120)]
    [InlineData(120, 120)]
    public void HoldSeconds_MapsDuration(double duration, double expected)
    {
        Assert.Equal(expected, HeadhunterCutsceneWatch.HoldSeconds(duration), 6);
    }

    [Fact]
    public void Start_MakesActive()
    {
        _watch.Start("FakeA", 20, Begin);

        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_Inactive_False()
    {
        bool ended = _watch.TryEnd(HeadhunterCutsceneState.Missing, Begin, out _);

        Assert.False(ended);
    }

    [Fact]
    public void TryEnd_PlayingBeforeDeadline_False()
    {
        _watch.Start("FakeA", 20, Begin);

        bool ended = _watch.TryEnd(HeadhunterCutsceneState.Playing, 31.9, out _);

        Assert.False(ended);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_PlayingAtDeadline_ByDuration()
    {
        _watch.Start("FakeA", 20, Begin);

        bool ended = _watch.TryEnd(
            HeadhunterCutsceneState.Playing,
            32,
            out HeadhunterCutsceneStop stop
        );

        Assert.True(ended);
        Assert.Equal(
            new HeadhunterCutsceneStop(
                new HeadhunterCutscene("FakeA", 20, Begin),
                22,
                HeadhunterCutsceneEnd.Duration
            ),
            stop
        );
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_StoppedWithinGrace_False()
    {
        _watch.Start("FakeA", 20, Begin);

        bool ended = _watch.TryEnd(HeadhunterCutsceneState.Stopped, 10.5, out _);

        Assert.False(ended);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_StoppedAfterGrace_ByStopped()
    {
        _watch.Start("FakeA", 20, Begin);

        bool ended = _watch.TryEnd(
            HeadhunterCutsceneState.Stopped,
            15,
            out HeadhunterCutsceneStop stop
        );

        Assert.True(ended);
        Assert.Equal(
            new HeadhunterCutsceneStop(
                new HeadhunterCutscene("FakeA", 20, Begin),
                5,
                HeadhunterCutsceneEnd.Stopped
            ),
            stop
        );
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_StoppedAtGrace_ByStopped()
    {
        _watch.Start("FakeA", 20, Begin);

        bool ended = _watch.TryEnd(
            HeadhunterCutsceneState.Stopped,
            Begin + 1,
            out HeadhunterCutsceneStop stop
        );

        Assert.True(ended);
        Assert.Equal(
            new HeadhunterCutsceneStop(
                new HeadhunterCutscene("FakeA", 20, Begin),
                1,
                HeadhunterCutsceneEnd.Stopped
            ),
            stop
        );
    }

    [Fact]
    public void TryEnd_Missing_ByMissingAtOnce()
    {
        _watch.Start("FakeA", 20, Begin);

        bool ended = _watch.TryEnd(
            HeadhunterCutsceneState.Missing,
            10.25,
            out HeadhunterCutsceneStop stop
        );

        Assert.True(ended);
        Assert.Equal(
            new HeadhunterCutsceneStop(
                new HeadhunterCutscene("FakeA", 20, Begin),
                0.25,
                HeadhunterCutsceneEnd.Missing
            ),
            stop
        );
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_ZeroDuration_UsesFallback()
    {
        _watch.Start("FakeA", 0, Begin);

        bool early = _watch.TryEnd(HeadhunterCutsceneState.Playing, 41.9, out _);
        bool late = _watch.TryEnd(
            HeadhunterCutsceneState.Playing,
            42,
            out HeadhunterCutsceneStop stop
        );

        Assert.False(early);
        Assert.True(late);
        Assert.Equal(HeadhunterCutsceneEnd.Duration, stop.By);
    }

    [Fact]
    public void TryEnd_HugeDuration_Capped()
    {
        _watch.Start("FakeA", 500, Begin);

        bool ended = _watch.TryEnd(HeadhunterCutsceneState.Playing, 132, out _);

        Assert.True(ended);
    }

    [Fact]
    public void Start_Twice_LatestWins()
    {
        _watch.Start("FakeA", 20, Begin);
        _watch.Start("FakeB", 5, 15);

        bool ended = _watch.TryEnd(
            HeadhunterCutsceneState.Playing,
            22,
            out HeadhunterCutsceneStop stop
        );

        Assert.True(ended);
        Assert.Equal(
            new HeadhunterCutsceneStop(
                new HeadhunterCutscene("FakeB", 5, 15),
                7,
                HeadhunterCutsceneEnd.Duration
            ),
            stop
        );
    }

    [Fact]
    public void TryPeek_Active_ReportsWithoutEnding()
    {
        _watch.Start("FakeA", 20, Begin);

        bool peeked = _watch.TryPeek(
            13,
            HeadhunterCutsceneEnd.Scene,
            out HeadhunterCutsceneStop stop
        );

        Assert.True(peeked);
        Assert.Equal(
            new HeadhunterCutsceneStop(
                new HeadhunterCutscene("FakeA", 20, Begin),
                3,
                HeadhunterCutsceneEnd.Scene
            ),
            stop
        );
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryPeek_Inactive_False()
    {
        bool peeked = _watch.TryPeek(13, HeadhunterCutsceneEnd.Scene, out _);

        Assert.False(peeked);
    }

    [Fact]
    public void Reset_EndsHold()
    {
        _watch.Start("FakeA", 20, Begin);

        _watch.Reset();

        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void Reset_Inactive_StaysInactive()
    {
        _watch.Reset();

        Assert.False(_watch.IsActive);
    }
}
