using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven;

/// <summary>Marks the span of ReceiveRefundAllWeaverTreeCommand so its favor spends can be waived.</summary>
[HarmonyPatch(typeof(LocalTreeData), "ReceiveRefundAllWeaverTreeCommand")]
public class WovenRefundAllScopePatch
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
