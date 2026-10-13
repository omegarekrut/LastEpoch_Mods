using HarmonyLib;
using Il2CppLE.Factions;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven;

/// <summary>Waives the favor a Weaver respec spends when Free Respec is on.</summary>
[HarmonyPatch(typeof(Faction), "TrySpendFavor")]
public class WovenRespecSpendPatch
{
    [HarmonyPrefix]
    private static void Prefix(ref int __0)
    {
        if (!Faction_Woven_FreeRespe.Rules.InRespec)
        {
            return;
        }

        __0 = Faction_Woven_FreeRespe.Rules.Spend(__0, Faction_Woven_FreeRespe.CanRun());
    }
}
