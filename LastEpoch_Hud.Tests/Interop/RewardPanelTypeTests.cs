using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause.RewardMenu;
using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Interop;

/// <summary>Every reward panel name is a game panel type.</summary>
public sealed class RewardPanelTypeTests
{
    private const string PanelBase = "Il2CppLE.UI.PanelSystem.Panel";

    [Fact]
    public void TypeNames_AreGamePanels()
    {
        GameEnvironment.SkipWithoutGame();
        ModuleDefinition game = GameEnvironment.ReadGameModule(
            Path.Combine(GameEnvironment.Il2CppDir, "Il2CppLE.dll")
        );

        string[] bad = HeadhunterRewardPanels
            .TypeNames.Where(n =>
                game.GetTypes().Count(t => t.Name == n && DerivesFromPanel(t)) != 1
            )
            .ToArray();

        Assert.Empty(bad);
    }

    private static bool DerivesFromPanel(TypeDefinition type)
    {
        TypeReference current = type.BaseType;
        while (current != null)
        {
            TypeReference element = current is GenericInstanceType generic
                ? generic.ElementType
                : current;
            if (element.FullName == PanelBase)
            {
                return true;
            }

            current = element.Resolve()?.BaseType;
        }

        return false;
    }
}
