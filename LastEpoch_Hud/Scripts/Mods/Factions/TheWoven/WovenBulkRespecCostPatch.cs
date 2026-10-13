using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.Factions;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven;

/// <summary>Makes the bulk Weaver tree update cost nothing when Free Respec is on.</summary>
[HarmonyPatch(typeof(LocalTreeData), "CalculateWeaverRespecCostForBulkUpdate")]
public class WovenBulkRespecCostPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref int __result)
    {
        __result = WovenFreeRespec.Cost(__result, Faction_Woven_FreeRespe.CanRun());
    }
}
