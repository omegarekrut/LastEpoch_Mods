using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven;

/// <summary>Marks the span of ReceiveRespecWeaverPointsCommand so its favor spends can be waived.</summary>
[HarmonyPatch(
    typeof(LocalTreeData),
    "ReceiveRespecWeaverPointsCommand",
    new[] { typeof(Il2CppStructArray<byte>) }
)]
public class WovenRespecPointsScopePatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        Faction_Woven_FreeRespe.Rules.Enter();
    }

    [HarmonyFinalizer]
    private static void Finalizer()
    {
        Faction_Woven_FreeRespe.Rules.Exit();
    }
}
