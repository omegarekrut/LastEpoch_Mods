using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterZonePauseTests
{
    private const string Hostile = "FakeA";
    private const string Town = "FakeB";
    private const double Start = 10;
    private const double Poll = HeadhunterZonePause.PollSeconds;
    private static readonly float[] _live = [5.5f, 0f, 3f];

    private readonly HeadhunterResolvedConfig _config = PauseTestData.Config(3);
    private readonly HeadhunterZonePause _zone = new();

    [Fact]
    public void OnScene_Hostile_PausesAtOnce()
    {
        HeadhunterPauseChange change = _zone.OnScene(Hostile, false, Start);

        Assert.Equal(HeadhunterPauseChange.Paused, change);
        Assert.True(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void TryEndArrival_Damageable_Resumes()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.Freeze.Apply(_config, _live);

        bool ended = _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 2, out _);

        Assert.True(ended);
        Assert.False(_zone.Freeze.IsPaused);
        Assert.True(_zone.Freeze.IsApplyPending);
    }

    [Fact]
    public void TryEndArrival_Protected_StaysPaused()
    {
        _zone.OnScene(Hostile, false, Start);

        bool ended = _zone.TryEndArrival(HeadhunterArrivalState.Protected, Start + 2, out _);

        Assert.False(ended);
        Assert.True(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Clear_DuringArrival_NotPausedNotHolding()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.Freeze.Apply(_config, _live);

        _zone.Clear();

        Assert.False(_zone.Freeze.IsPaused);
        Assert.False(_zone.Freeze.IsHolding);
        Assert.False(_zone.Freeze.IsApplyPending);
    }

    [Fact]
    public void Clear_InCombatZone_KeepsPolling()
    {
        _zone.OnScene(Hostile, false, Start);

        _zone.Clear();

        Assert.True(_zone.IsPollDue(Start + 100));
        Assert.False(_zone.IsWatchingArrival);
    }

    [Fact]
    public void IsPollDue_BeforeAnyScene_False()
    {
        Assert.False(_zone.IsPollDue(100));
    }

    [Fact]
    public void IsPollDue_NonCombat_False()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.OnScene(Town, true, Start + 1);

        Assert.False(_zone.IsPollDue(Start + 100));
    }

    [Fact]
    public void IsPollDue_Hostile_FirstAtIntervalThenThrottled()
    {
        _zone.OnScene(Hostile, false, Start);

        Assert.False(_zone.IsPollDue(Start + 0.1));
        Assert.True(_zone.IsPollDue(Start + Poll));
        Assert.False(_zone.IsPollDue(Start + Poll + 0.05));
        Assert.True(_zone.IsPollDue(Start + (2 * Poll)));
    }

    [Fact]
    public void IsPollDue_AfterLongGap_ThrottlesFromNow()
    {
        _zone.OnScene(Hostile, false, Start);

        Assert.True(_zone.IsPollDue(Start + 5));
        Assert.False(_zone.IsPollDue(Start + 5 + 0.1));
        Assert.True(_zone.IsPollDue(Start + 5 + Poll));
    }

    [Fact]
    public void IsPollDue_AfterArrivalEnded_StillPolls()
    {
        StartHostileAndEndArrival();

        Assert.True(_zone.IsPollDue(Start + 1));
        Assert.True(_zone.IsPollDue(Start + 100));
    }

    [Fact]
    public void IsWatchingArrival_HostileThenEnded()
    {
        _zone.OnScene(Hostile, false, Start);
        Assert.True(_zone.IsWatchingArrival);

        _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 1, out _);
        Assert.False(_zone.IsWatchingArrival);

        _zone.OnScene(Town, true, Start + 2);
        Assert.False(_zone.IsWatchingArrival);
    }

    [Fact]
    public void TryCinematic_StartAndEnd_PausesThenResumes()
    {
        StartHostileAndEndArrival();

        bool started = _zone.TryCinematic(true, Start + 2, out double heldAtStart);

        Assert.True(started);
        Assert.Equal(0, heldAtStart, 6);
        Assert.True(_zone.Freeze.IsPaused);
        Assert.True(_zone.Freeze.IsApplyPending);

        _zone.Freeze.Apply(_config, _live);
        bool ended = _zone.TryCinematic(false, Start + 4.5, out double held);

        Assert.True(ended);
        Assert.Equal(2.5, held, 6);
        Assert.False(_zone.Freeze.IsPaused);
        Assert.True(_zone.Freeze.IsApplyPending);
    }

    [Fact]
    public void TryCinematic_SameValue_NoChange()
    {
        StartHostileAndEndArrival();
        _zone.TryCinematic(true, Start + 2, out _);

        bool again = _zone.TryCinematic(true, Start + 3, out _);

        Assert.False(again);
        Assert.True(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void TryCinematic_FalseWhenIdle_False()
    {
        StartHostileAndEndArrival();

        Assert.False(_zone.TryCinematic(false, Start + 2, out _));
    }

    [Fact]
    public void TryCinematic_DuringArrival_StaysPausedUntilBothEnd()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.TryCinematic(true, Start + 1, out _);
        _zone.TryCinematic(false, Start + 2, out _);

        Assert.True(_zone.Freeze.IsPaused);

        _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 3, out _);

        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void TryEndArrival_DuringCinematic_StaysPaused()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.TryCinematic(true, Start + 1, out _);

        bool ended = _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 2, out _);

        Assert.True(ended);
        Assert.True(_zone.Freeze.IsPaused);

        _zone.TryCinematic(false, Start + 3, out _);

        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void OnScene_ResetsCinematic()
    {
        StartHostileAndEndArrival();
        _zone.TryCinematic(true, Start + 2, out _);

        _zone.OnScene(Hostile, false, Start + 3);
        _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 4, out _);

        Assert.False(_zone.Freeze.IsPaused);
        Assert.False(_zone.TryCinematic(false, Start + 5, out _));
    }

    [Fact]
    public void Clear_DuringCinematic_NotPausedNotHolding()
    {
        StartHostileAndEndArrival();
        _zone.TryCinematic(true, Start + 2, out _);
        _zone.Freeze.Apply(_config, _live);

        _zone.Clear();

        Assert.False(_zone.Freeze.IsPaused);
        Assert.False(_zone.Freeze.IsHolding);
        Assert.False(_zone.Freeze.IsApplyPending);
        Assert.False(_zone.TryCinematic(false, Start + 3, out _));
    }

    [Fact]
    public void Clear_InNonCombatZone_StaysPaused()
    {
        _zone.OnScene(Town, true, Start);
        _zone.Freeze.Apply(_config, _live);

        _zone.Clear();

        Assert.True(_zone.Freeze.IsPaused);
        Assert.True(_zone.Freeze.IsHolding);
    }

    [Fact]
    public void OnScene_TownThenHostile_StaysPausedUntilArrivalEnds()
    {
        _zone.OnScene(Town, true, Start);
        _zone.Freeze.Apply(_config, _live);

        HeadhunterPauseChange change = _zone.OnScene(Hostile, false, Start + 1);

        Assert.Equal(HeadhunterPauseChange.None, change);
        Assert.True(_zone.Freeze.IsPaused);

        _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 3, out _);

        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void BossIntro_Long_PausesThenEndResumes()
    {
        StartHostileAndEndArrival();

        bool started = _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 2);

        Assert.True(started);
        Assert.True(_zone.HasBossIntro);
        Assert.True(_zone.Freeze.IsPaused);

        bool ended = _zone.TryEndBossIntro(1, Start + 13, out _, out _);

        Assert.True(ended);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void BossIntro_Short_NoPause()
    {
        StartHostileAndEndArrival();

        bool started = _zone.TryStartBossIntro(1, "FakeA", 2f, Start + 2);

        Assert.False(started);
        Assert.False(_zone.Freeze.IsPaused);
        Assert.False(_zone.HasBossIntro);
    }

    [Fact]
    public void BossIntro_NonCombatZone_Ignored()
    {
        _zone.OnScene(Town, true, Start);

        bool started = _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 1);

        Assert.False(started);
        Assert.False(_zone.HasBossIntro);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BossIntro_WithCinematic_ResumesOnlyWhenBothEnd(bool introEndsFirst)
    {
        StartHostileAndEndArrival();
        _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 2);
        _zone.TryCinematic(true, Start + 2, out _);

        EndOne(introEndsFirst);
        Assert.True(_zone.Freeze.IsPaused);

        EndOne(!introEndsFirst);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void BossIntro_DuringArrival_StaysPausedAfterIntroEnds()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 1);

        _zone.TryEndBossIntro(1, Start + 5, out _, out _);

        Assert.True(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void TryExpireBossIntro_Overdue_Resumes()
    {
        StartHostileAndEndArrival();
        _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 2);

        bool expired = _zone.TryExpireBossIntro(Start + 16, out _, out _);

        Assert.True(expired);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void BossIntro_NotDueOrUnknown_StaysPaused()
    {
        StartHostileAndEndArrival();
        _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 2);

        bool expired = _zone.TryExpireBossIntro(Start + 15.9, out _, out _);
        bool ended = _zone.TryEndBossIntro(99, Start + 5, out _, out _);

        Assert.False(expired);
        Assert.False(ended);
        Assert.True(_zone.Freeze.IsPaused);
        Assert.True(_zone.HasBossIntro);
    }

    [Fact]
    public void OnScene_DropsBossIntro()
    {
        StartHostileAndEndArrival();
        _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 2);

        _zone.OnScene(Hostile, false, Start + 3);
        _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 4, out _);

        Assert.False(_zone.HasBossIntro);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Clear_DropsBossIntro()
    {
        StartHostileAndEndArrival();
        _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 2);

        _zone.Clear();

        Assert.False(_zone.HasBossIntro);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Cutscene_Hostile_PausesThenStopResumes()
    {
        StartHostileAndEndArrival();

        bool started = _zone.TryStartCutscene("FakeA", 20, Start + 2);

        Assert.True(started);
        Assert.True(_zone.HasCutscene);
        Assert.True(_zone.Freeze.IsPaused);

        bool ended = _zone.TryEndCutscene(
            HeadhunterCutsceneState.Stopped,
            Start + 7,
            out HeadhunterCutsceneStop stop
        );

        Assert.True(ended);
        Assert.Equal(HeadhunterCutsceneEnd.Stopped, stop.By);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Cutscene_Playing_StaysPaused()
    {
        StartHostileAndEndArrival();
        _zone.TryStartCutscene("FakeA", 20, Start + 2);

        bool ended = _zone.TryEndCutscene(HeadhunterCutsceneState.Playing, Start + 5, out _);

        Assert.False(ended);
        Assert.True(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Cutscene_NonCombatZone_Ignored()
    {
        _zone.OnScene(Town, true, Start);

        bool started = _zone.TryStartCutscene("FakeA", 20, Start + 1);

        Assert.False(started);
        Assert.False(_zone.HasCutscene);
    }

    [Fact]
    public void Cutscene_WithBossIntro_ResumesOnlyWhenBothEnd()
    {
        StartHostileAndEndArrival();
        _zone.TryStartBossIntro(1, "FakeA", 12f, Start + 2);
        _zone.TryStartCutscene("FakeA", 20, Start + 2);

        bool ended = _zone.TryEndCutscene(HeadhunterCutsceneState.Missing, Start + 5, out _);
        Assert.True(ended);
        Assert.False(_zone.HasCutscene);
        Assert.True(_zone.Freeze.IsPaused);

        _zone.TryEndBossIntro(1, Start + 6, out _, out _);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Cutscene_DuringArrival_StaysPausedAfterEnd()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.TryStartCutscene("FakeA", 20, Start + 0.5);

        bool ended = _zone.TryEndCutscene(HeadhunterCutsceneState.Missing, Start + 5, out _);

        Assert.True(ended);
        Assert.False(_zone.HasCutscene);
        Assert.True(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Cutscene_DuringCinematic_StaysPausedAfterEnd()
    {
        StartHostileAndEndArrival();
        _zone.TryCinematic(true, Start + 2, out _);
        _zone.TryStartCutscene("FakeA", 20, Start + 2);

        bool ended = _zone.TryEndCutscene(HeadhunterCutsceneState.Missing, Start + 5, out _);

        Assert.True(ended);
        Assert.False(_zone.HasCutscene);
        Assert.True(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void TryPeekCutscene_LeavesFreeze()
    {
        StartHostileAndEndArrival();
        _zone.TryStartCutscene("FakeA", 20, Start + 2);
        _zone.Freeze.Apply(_config, _live);
        bool pendingBefore = _zone.Freeze.IsApplyPending;

        bool peeked = _zone.TryPeekCutscene(Start + 4, HeadhunterCutsceneEnd.Scene, out _);

        Assert.True(peeked);
        Assert.True(_zone.Freeze.IsPaused);
        Assert.True(_zone.HasCutscene);
        Assert.Equal(pendingBefore, _zone.Freeze.IsApplyPending);
    }

    [Fact]
    public void OnScene_DropsCutscene()
    {
        StartHostileAndEndArrival();
        _zone.TryStartCutscene("FakeA", 20, Start + 2);

        _zone.OnScene(Hostile, false, Start + 3);
        _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 4, out _);

        Assert.False(_zone.HasCutscene);
        Assert.False(_zone.Freeze.IsPaused);
    }

    [Fact]
    public void Clear_DropsCutscene()
    {
        StartHostileAndEndArrival();
        _zone.TryStartCutscene("FakeA", 20, Start + 2);

        _zone.Clear();

        Assert.False(_zone.HasCutscene);
        Assert.False(_zone.Freeze.IsPaused);
    }

    private void EndOne(bool intro)
    {
        if (intro)
        {
            _zone.TryEndBossIntro(1, Start + 5, out _, out _);
            return;
        }

        _zone.TryCinematic(false, Start + 5, out _);
    }

    private void StartHostileAndEndArrival()
    {
        _zone.OnScene(Hostile, false, Start);
        _zone.TryEndArrival(HeadhunterArrivalState.Damageable, Start + 1, out _);
    }
}
