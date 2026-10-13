namespace LastEpoch_Hud.Tests.Patches;

public sealed class FoldedAliasesTests
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _aliases =
        new Dictionary<string, IReadOnlyList<string>> { ["T::a"] = ["T::b"] };

    [Fact]
    public void Unexpected_OnlyAlias_Empty()
    {
        Assert.Empty(FoldedAliases.Unexpected(_aliases, "T::a", ["T::b"]));
    }

    [Fact]
    public void Unexpected_AliasAndOther_KeepsOther()
    {
        Assert.Equal(["U::c"], FoldedAliases.Unexpected(_aliases, "T::a", ["T::b", "U::c"]));
    }

    [Fact]
    public void Unexpected_TargetNotListed_KeepsAll()
    {
        Assert.Equal(["T::b"], FoldedAliases.Unexpected(_aliases, "T::x", ["T::b"]));
    }

    [Fact]
    public void Unexpected_ManyOthers_KeepsOrder()
    {
        Assert.Equal(
            ["Z::z", "A::a"],
            FoldedAliases.Unexpected(_aliases, "T::a", ["Z::z", "T::b", "A::a"])
        );
    }

    [Fact]
    public void Stale_AliasStillFolded_Empty()
    {
        Assert.Empty(FoldedAliases.Stale(_aliases, "T::a", ["T::b"]));
    }

    [Fact]
    public void Stale_AliasGone_ListsIt()
    {
        Assert.Equal(["T::b"], FoldedAliases.Stale(_aliases, "T::a", []));
    }

    [Fact]
    public void Stale_TargetNotListed_Empty()
    {
        Assert.Empty(FoldedAliases.Stale(_aliases, "T::x", []));
    }
}
