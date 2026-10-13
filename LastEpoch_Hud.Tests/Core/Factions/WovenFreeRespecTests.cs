using LastEpoch_Hud.Scripts.Core.Factions;

namespace LastEpoch_Hud.Tests.Core.Factions;

/// <summary>Favor rules for Woven Free Respec.</summary>
public sealed class WovenFreeRespecTests
{
    [Fact]
    public void Spend_OutsideRespec_ReturnsAmount()
    {
        WovenFreeRespec rules = new();

        Assert.Equal(20, rules.Spend(20, true));
    }

    [Fact]
    public void Spend_InRespecEnabled_ReturnsZero()
    {
        WovenFreeRespec rules = new();
        rules.Enter();

        Assert.Equal(0, rules.Spend(40, true));
    }

    [Fact]
    public void Spend_InRespecDisabled_ReturnsAmount()
    {
        WovenFreeRespec rules = new();
        rules.Enter();

        Assert.Equal(40, rules.Spend(40, false));
    }

    [Fact]
    public void Exit_AfterEnter_LeavesRespec()
    {
        WovenFreeRespec rules = new();
        rules.Enter();
        rules.Exit();

        Assert.False(rules.InRespec);
        Assert.Equal(20, rules.Spend(20, true));
    }

    [Fact]
    public void Enter_Nested_StaysInRespecUntilLastExit()
    {
        WovenFreeRespec rules = new();
        rules.Enter();
        rules.Enter();
        rules.Exit();

        Assert.True(rules.InRespec);

        rules.Exit();

        Assert.False(rules.InRespec);
    }

    [Fact]
    public void Exit_WithoutEnter_DoesNotGoNegative()
    {
        WovenFreeRespec rules = new();
        rules.Exit();
        rules.Exit();
        rules.Enter();

        Assert.True(rules.InRespec);
    }

    [Fact]
    public void Cost_Enabled_ReturnsZero()
    {
        Assert.Equal(0, WovenFreeRespec.Cost(60, true));
    }

    [Fact]
    public void Cost_Disabled_ReturnsCost()
    {
        Assert.Equal(60, WovenFreeRespec.Cost(60, false));
    }
}
