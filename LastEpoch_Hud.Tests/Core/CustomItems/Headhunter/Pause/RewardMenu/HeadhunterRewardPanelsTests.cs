using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause.RewardMenu;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause.RewardMenu;

public sealed class HeadhunterRewardPanelsTests
{
    private static readonly string[] _chosen =
    [
        "ChooseBlessingPanel",
        "ChooseEchoRewardPanel",
        "NemesisPanel",
        "RageOfMorditasRewardPanel",
        "GreaterWindowPanel",
        "TimeBeastEvolutionPanel",
        "VaultsOfUncertainFatePanel",
        "SanctuaryOfEterraPanel",
        "IdolEnchantmentPanel",
        "GlyphOfInsightReplacementPanel",
        "RandomizeLegendaryPotentialPanel",
        "ShatterSetItemsPanel",
        "SwapAttributesPanel",
        "RerollUniqueAffixesPanel",
        "ExchangeWovenEchoesPanel",
    ];

    [Fact]
    public void TypeNames_AreTheChosenSet()
    {
        Assert.Equal(_chosen.Length, HeadhunterRewardPanels.TypeNames.Count);
        Assert.True(new HashSet<string>(_chosen).SetEquals(HeadhunterRewardPanels.TypeNames));
    }

    [Fact]
    public void TypeNames_AllAreRewardChoices()
    {
        Assert.All(
            HeadhunterRewardPanels.TypeNames,
            name => Assert.True(HeadhunterRewardPanels.IsRewardChoice(name))
        );
    }

    [Theory]
    [InlineData("ChooseBlessingPanel")]
    [InlineData("ChooseEchoRewardPanel")]
    [InlineData("NemesisPanel")]
    public void IsRewardChoice_UserNamed_True(string name)
    {
        Assert.True(HeadhunterRewardPanels.IsRewardChoice(name));
    }

    [Theory]
    [InlineData("QuestRewardPanel")]
    [InlineData("MonolithCompletePanel")]
    [InlineData("InventoryPanel")]
    [InlineData("StashPanel")]
    [InlineData("PassivesPanel")]
    [InlineData("SkillsPanel")]
    [InlineData("FullscreenPanelSkillTree")]
    [InlineData("FactionsPanel")]
    [InlineData("ObservatoryPanel")]
    [InlineData("ChooseItemFactionPanel")]
    [InlineData("ChooseMasteryPanel")]
    public void IsRewardChoice_Menus_False(string name)
    {
        Assert.False(HeadhunterRewardPanels.IsRewardChoice(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("chooseblessingpanel")]
    public void IsRewardChoice_NullEmptyOrWrongCase_False(string name)
    {
        Assert.False(HeadhunterRewardPanels.IsRewardChoice(name));
    }
}
