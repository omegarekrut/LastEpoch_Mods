namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

/// <summary>Combines zone type, arrival protection, cinematics, boss intros and cutscenes into the HH timer freeze.</summary>
public sealed class HeadhunterZonePause
{
    public const double PollSeconds = 0.25;

    private readonly HeadhunterArrivalWatch _arrival = new();
    private readonly HeadhunterCinematicWatch _cinematic = new();
    private readonly HeadhunterBossIntroWatch _intros = new();
    private readonly HeadhunterCutsceneWatch _cutscene = new();
    private bool _nonCombat;
    private double _nextPoll = double.MaxValue;

    public HeadhunterTimerFreeze Freeze { get; } = new();

    public string Scene => _arrival.Scene;

    public bool IsWatchingArrival => _arrival.IsWatching;

    public bool HasBossIntro => _intros.IsActive;

    public bool HasCutscene => _cutscene.IsActive;

    public HeadhunterPauseChange OnScene(string scene, bool nonCombat, double now)
    {
        _nonCombat = nonCombat;
        _cinematic.Reset();
        _intros.Reset();
        _cutscene.Reset();
        _arrival.Begin(scene, nonCombat, now);
        _nextPoll = nonCombat ? double.MaxValue : now + PollSeconds;
        return Freeze.Request(IsPausedNow());
    }

    /// <summary>True in a combat zone when the poll interval passed; schedules the next poll.</summary>
    public bool IsPollDue(double now)
    {
        if (now < _nextPoll)
        {
            return false;
        }

        _nextPoll = now + PollSeconds;
        return true;
    }

    public bool TryEndArrival(HeadhunterArrivalState state, double now, out double heldSeconds)
    {
        if (!_arrival.TryEnd(state, now, out heldSeconds))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Applies a cinematic start or end to the freeze; true when the flag changed.</summary>
    public bool TryCinematic(bool active, double now, out double heldSeconds)
    {
        if (!_cinematic.TryChange(active, now, out heldSeconds))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Tracks a long boss intro; false for short intros and non-combat zones.</summary>
    public bool TryStartBossIntro(long id, string actor, float durationSeconds, double now)
    {
        if (_nonCombat || !_intros.TryStart(id, actor, durationSeconds, now))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Stops tracking an intro on emerge end; true when it was tracked.</summary>
    public bool TryEndBossIntro(
        long id,
        double now,
        out HeadhunterBossIntro intro,
        out double heldSeconds
    )
    {
        if (!_intros.TryEnd(id, now, out intro, out heldSeconds))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Drops one overdue intro whose end was never seen; true when one expired.</summary>
    public bool TryExpireBossIntro(
        double now,
        out HeadhunterBossIntro intro,
        out double heldSeconds
    )
    {
        if (!_intros.TryExpire(now, out intro, out heldSeconds))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Tracks a cutscene (latest wins); false in non-combat zones.</summary>
    public bool TryStartCutscene(string id, double durationSeconds, double now)
    {
        if (_nonCombat)
        {
            return false;
        }

        _cutscene.Start(id, durationSeconds, now);
        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Ends the cutscene hold when the game state or deadline says so.</summary>
    public bool TryEndCutscene(
        HeadhunterCutsceneState state,
        double now,
        out HeadhunterCutsceneStop stop
    )
    {
        if (!_cutscene.TryEnd(state, now, out stop))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>The stop ending the cutscene now would give; leaves the freeze unchanged.</summary>
    public bool TryPeekCutscene(
        double now,
        HeadhunterCutsceneEnd by,
        out HeadhunterCutsceneStop stop
    )
    {
        return _cutscene.TryPeek(now, by, out stop);
    }

    /// <summary>Drops the arrival watch, the cinematic, boss intros, the cutscene and kept timers after HH buffs were removed.</summary>
    public void Clear()
    {
        _arrival.Cancel();
        _cinematic.Reset();
        _intros.Reset();
        _cutscene.Reset();
        Freeze.Request(IsPausedNow());
        Freeze.ClearTimers();
    }

    private bool IsPausedNow()
    {
        return HeadhunterPauseRule.IsPaused(
            _nonCombat,
            _arrival.IsWatching,
            _cinematic.IsActive,
            _intros.IsActive,
            _cutscene.IsActive
        );
    }
}
