using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterPauseRuleTests
{
    [Theory]
    [InlineData(false, false, false, false, false, false, false)]
    [InlineData(true, false, false, false, false, false, true)]
    [InlineData(false, true, false, false, false, false, true)]
    [InlineData(false, false, true, false, false, false, true)]
    [InlineData(false, false, false, true, false, false, true)]
    [InlineData(false, false, false, false, true, false, true)]
    [InlineData(false, false, false, false, false, true, true)]
    [InlineData(true, true, true, true, true, true, true)]
    public void IsPaused_IsAnyOfSix(
        bool nonCombat,
        bool arrival,
        bool cinematic,
        bool bossIntro,
        bool cutscene,
        bool rewardMenu,
        bool expected
    )
    {
        Assert.Equal(
            expected,
            HeadhunterPauseRule.IsPaused(
                nonCombat,
                arrival,
                cinematic,
                bossIntro,
                cutscene,
                rewardMenu
            )
        );
    }
}
