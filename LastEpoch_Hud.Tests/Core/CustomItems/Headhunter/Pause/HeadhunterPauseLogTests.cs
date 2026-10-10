using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause.RewardMenu;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterPauseLogTests
{
    private const string FakeScene = "FakeScene";

    [Theory]
    [InlineData(true, HeadhunterPauseChange.Paused, "nonCombat=yes", "timers=paused")]
    [InlineData(false, HeadhunterPauseChange.Resumed, "nonCombat=no", "timers=resumed")]
    [InlineData(true, HeadhunterPauseChange.None, "nonCombat=yes", "timers=unchanged")]
    [InlineData(false, HeadhunterPauseChange.None, "nonCombat=no", "timers=unchanged")]
    public void Zone_HasSceneNonCombatAndChange(
        bool nonCombat,
        HeadhunterPauseChange change,
        string expectedZone,
        string expectedTimers
    )
    {
        string line = HeadhunterPauseLog.Zone(FakeScene, nonCombat, change);

        Assert.Contains(FakeScene, line);
        Assert.Contains(expectedZone, line);
        Assert.Contains(expectedTimers, line);
    }

    [Theory]
    [InlineData(true, "held", "released")]
    [InlineData(false, "released", "held")]
    public void Applied_HasStateAndCount(bool holding, string expected, string opposite)
    {
        string line = HeadhunterPauseLog.Applied(holding, 42);

        Assert.Contains(expected, line);
        Assert.DoesNotContain(opposite, line);
        Assert.Contains("42", line);
    }

    [Theory]
    [InlineData(HeadhunterArrivalState.Damageable, "by=damageable")]
    [InlineData(HeadhunterArrivalState.Missing, "by=missing")]
    public void Arrival_HasSceneHeldAndCause(HeadhunterArrivalState state, string expectedCause)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            string line = HeadhunterPauseLog.Arrival(FakeScene, 2.5, state);

            Assert.Contains($"scene={FakeScene}", line);
            Assert.Contains("held=2.50s", line);
            Assert.Contains(expectedCause, line);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Cinematic_Started_HasSceneNoHeld()
    {
        string line = HeadhunterPauseLog.Cinematic(FakeScene, true, 0);

        Assert.Contains($"scene={FakeScene}", line);
        Assert.DoesNotContain("held=", line);
    }

    [Fact]
    public void Cinematic_Ended_HasSceneAndHeld()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            string line = HeadhunterPauseLog.Cinematic(FakeScene, false, 2.5);

            Assert.Contains($"scene={FakeScene}", line);
            Assert.Contains("held=2.50s", line);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void BossIntroStarted_HasSceneActorDuration()
    {
        string line = InGermanCulture(() =>
            HeadhunterPauseLog.BossIntroStarted(FakeScene, Intro())
        );

        Assert.Contains($"scene={FakeScene}", line);
        Assert.Contains("actor=FakeA", line);
        Assert.Contains("duration=12.00s", line);
        Assert.DoesNotContain("held=", line);
    }

    [Fact]
    public void BossIntroEnded_HasHeld()
    {
        string line = InGermanCulture(() =>
            HeadhunterPauseLog.BossIntroEnded(FakeScene, Intro(), 11.5)
        );

        Assert.Contains($"scene={FakeScene}", line);
        Assert.Contains("actor=FakeA", line);
        Assert.Contains("duration=12.00s", line);
        Assert.Contains("held=11.50s", line);
    }

    [Fact]
    public void BossIntroExpired_HasHeldAndDiffersFromEnded()
    {
        string expired = InGermanCulture(() =>
            HeadhunterPauseLog.BossIntroExpired(FakeScene, Intro(), 11.5)
        );
        string ended = InGermanCulture(() =>
            HeadhunterPauseLog.BossIntroEnded(FakeScene, Intro(), 11.5)
        );

        Assert.Contains($"scene={FakeScene}", expired);
        Assert.Contains("actor=FakeA", expired);
        Assert.Contains("duration=12.00s", expired);
        Assert.Contains("held=11.50s", expired);
        Assert.Contains("expired", expired);
        Assert.NotEqual(ended, expired);
    }

    [Fact]
    public void CutsceneStarted_HasSceneIdDuration()
    {
        string line = InGermanCulture(() =>
            HeadhunterPauseLog.CutsceneStarted(FakeScene, Cutscene())
        );

        Assert.Contains($"scene={FakeScene}", line);
        Assert.Contains("id=FakeA", line);
        Assert.Contains("duration=69.17s", line);
        Assert.DoesNotContain("held=", line);
    }

    [Theory]
    [InlineData(HeadhunterCutsceneEnd.Duration, "by=duration")]
    [InlineData(HeadhunterCutsceneEnd.Stopped, "by=stopped")]
    [InlineData(HeadhunterCutsceneEnd.Missing, "by=missing")]
    [InlineData(HeadhunterCutsceneEnd.Scene, "by=scene")]
    public void CutsceneEnded_HasHeldAndBy(HeadhunterCutsceneEnd by, string expectedBy)
    {
        string line = InGermanCulture(() => HeadhunterPauseLog.CutsceneEnded(FakeScene, Stop(by)));

        Assert.Contains($"scene={FakeScene}", line);
        Assert.Contains("id=FakeA", line);
        Assert.Contains("duration=69.17s", line);
        Assert.Contains("held=11.50s", line);
        Assert.Contains(expectedBy, line);
    }

    [Fact]
    public void RewardMenuStarted_HasScenePanel()
    {
        string line = HeadhunterPauseLog.RewardMenuStarted(FakeScene, Menu());

        Assert.Contains($"scene={FakeScene}", line);
        Assert.Contains("panel=FakePanelA", line);
        Assert.DoesNotContain("held=", line);
    }

    [Theory]
    [InlineData(HeadhunterRewardMenuEnd.Closed, "by=closed")]
    [InlineData(HeadhunterRewardMenuEnd.Hidden, "by=hidden")]
    [InlineData(HeadhunterRewardMenuEnd.Missing, "by=missing")]
    [InlineData(HeadhunterRewardMenuEnd.Cap, "by=cap")]
    [InlineData(HeadhunterRewardMenuEnd.Scene, "by=scene")]
    public void RewardMenuEnded_HasHeldAndBy(HeadhunterRewardMenuEnd by, string expectedBy)
    {
        string line = InGermanCulture(() =>
            HeadhunterPauseLog.RewardMenuEnded(FakeScene, RewardStop(by))
        );

        Assert.Contains($"scene={FakeScene}", line);
        Assert.Contains("panel=FakePanelA", line);
        Assert.Contains("held=11.50s", line);
        Assert.Contains(expectedBy, line);
    }

    [Fact]
    public void Lines_HaveNoNewline()
    {
        string lines =
            HeadhunterPauseLog.Zone(FakeScene, true, HeadhunterPauseChange.Paused)
            + HeadhunterPauseLog.Applied(true, 3)
            + HeadhunterPauseLog.Arrival(FakeScene, 1, HeadhunterArrivalState.Damageable)
            + HeadhunterPauseLog.Cinematic(FakeScene, true, 0)
            + HeadhunterPauseLog.Cinematic(FakeScene, false, 1)
            + HeadhunterPauseLog.BossIntroStarted(FakeScene, Intro())
            + HeadhunterPauseLog.BossIntroEnded(FakeScene, Intro(), 1)
            + HeadhunterPauseLog.BossIntroExpired(FakeScene, Intro(), 1)
            + HeadhunterPauseLog.CutsceneStarted(FakeScene, Cutscene())
            + HeadhunterPauseLog.CutsceneEnded(FakeScene, Stop(HeadhunterCutsceneEnd.Stopped))
            + HeadhunterPauseLog.RewardMenuStarted(FakeScene, Menu())
            + HeadhunterPauseLog.RewardMenuEnded(
                FakeScene,
                RewardStop(HeadhunterRewardMenuEnd.Closed)
            );

        Assert.DoesNotContain('\n', lines);
        Assert.DoesNotContain('\r', lines);
    }

    private static HeadhunterRewardMenu Menu()
    {
        return new HeadhunterRewardMenu(1, "FakePanelA", 10);
    }

    private static HeadhunterRewardMenuStop RewardStop(HeadhunterRewardMenuEnd by)
    {
        return new HeadhunterRewardMenuStop(Menu(), 11.5, by);
    }

    private static HeadhunterCutscene Cutscene()
    {
        return new HeadhunterCutscene("FakeA", 69.17, 10);
    }

    private static HeadhunterCutsceneStop Stop(HeadhunterCutsceneEnd by)
    {
        return new HeadhunterCutsceneStop(Cutscene(), 11.5, by);
    }

    private static HeadhunterBossIntro Intro()
    {
        return new HeadhunterBossIntro(1, "FakeA", 12f, 10);
    }

    private static string InGermanCulture(Func<string> build)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            return build();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
