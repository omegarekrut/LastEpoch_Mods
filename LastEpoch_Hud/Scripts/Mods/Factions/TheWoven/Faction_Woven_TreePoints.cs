using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using LastEpoch_Hud.Scripts.Core.ModUI;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven;

public class Faction_Woven_TreePoints
{
    static bool overrideApplied;
    static bool usedOverride;
    static bool hasRealSnapshot;
    static ushort realSnapshot;
    static bool readingMax;
    static bool writing;

    public static int GameMax()
    {
        readingMax = true;
        int max = 0;
        try
        {
            max = TheWeaver.MaxWeaverPoints;
        }
        catch { }
        readingMax = false;
        if (max < 1)
        {
            return 0;
        }
        if (max > ushort.MaxValue)
        {
            return ushort.MaxValue;
        }
        return max;
    }

    public static bool CanRun() =>
        Scenes.IsGameScene() && ModSettings.FactionsWoven.TreePoints.Enabled;

    private static bool MultiplierOn() =>
        Scenes.IsGameScene() && ModSettings.FactionsWoven.PointMultiplier.Enabled;

    static bool Active()
    {
        return CanRun() || MultiplierOn();
    }

    private static int Multiplier() =>
        PointMultiplier.Clamp(ModSettings.FactionsWoven.PointMultiplier.Value);

    static int TargetPoints()
    {
        int points = BasePoints();
        if (!MultiplierOn())
        {
            return points;
        }
        return ClampPoints(points * Multiplier());
    }

    private static int BasePoints()
    {
        if (CanRun())
        {
            return ClampPoints((int)ModSettings.FactionsWoven.TreePoints.Value);
        }
        if (TryRealEarned(out int points))
        {
            return points;
        }
        return hasRealSnapshot ? realSnapshot : 0;
    }

    public static int ClampPoints(int points)
    {
        if (points < 0)
        {
            return 0;
        }
        if (points > ushort.MaxValue)
        {
            return ushort.MaxValue;
        }
        return points;
    }

    public static void SyncTreePoints()
    {
        if (ModSettings.FactionsWoven.TreePoints.Enabled)
        {
            ApplyToPlayer();
            return;
        }
        ReleaseToRealPoints();
    }

    public static void ReleaseToRealPoints()
    {
        usedOverride = true;
        overrideApplied = true;
        ApplyToPlayer();
    }

    public static void ApplyToPlayer()
    {
        try
        {
            if (
                !Refs_Manager.player_treedata.IsNullOrDestroyed()
                && Refs_Manager.player_treedata.weaverTree != null
            )
            {
                WritePoints(Refs_Manager.player_treedata.weaverTree);
            }
            RefreshOpenUi();
        }
        catch { }
    }

    static TheWeaver FindWeaver()
    {
        if (
            Refs_Manager.faction_tracker.IsNullOrDestroyed()
            && !Refs_Manager.player_actor.IsNullOrDestroyed()
        )
        {
            Refs_Manager.faction_tracker =
                Refs_Manager.player_actor.gameObject.GetComponent<FactionTracker>();
        }
        if (
            Refs_Manager.faction_tracker.IsNullOrDestroyed()
            || Refs_Manager.faction_tracker.factions == null
        )
        {
            return null;
        }

        foreach (
            Il2CppSystem.Collections.Generic.KeyValuePair<FactionID, Faction> values in Refs_Manager
                .faction_tracker
                .factions
        )
        {
            if (values.Key != FactionID.TheWeaver || values.Value == null)
            {
                continue;
            }
            TheWeaver weaver = values.Value.TryCast<TheWeaver>();
            if (weaver != null)
            {
                return weaver;
            }
        }
        return null;
    }

    static bool TryRealEarned(out int points)
    {
        points = 0;
        TheWeaver weaver = FindWeaver();
        if (weaver == null)
        {
            return false;
        }

        int earned = weaver.EarnedWeaverPointsFromRank + weaver.EarnedWeaverPointsFromWovenEchoes;
        points = ClampPoints(earned);
        return true;
    }

    static void WritePoints(LocalTreeData.WeaverTreeData tree)
    {
        if (writing || tree == null)
        {
            return;
        }
        writing = true;
        try
        {
            if (Active())
            {
                if (!hasRealSnapshot)
                {
                    int earned;
                    realSnapshot = (ushort)(
                        TryRealEarned(out earned)
                            ? earned
                            : tree._EarnedWeaverPoints_k__BackingField
                    );
                    hasRealSnapshot = true;
                }
                tree.EarnedWeaverPoints = (ushort)TargetPoints();
                overrideApplied = true;
                usedOverride = true;
            }
            else if (overrideApplied)
            {
                RestoreRealPoints(tree);
            }
        }
        finally
        {
            writing = false;
        }
    }

    static void RestoreRealPoints(LocalTreeData.WeaverTreeData tree)
    {
        if (tree == null)
        {
            return;
        }
        int points;
        if (!TryRealEarned(out points))
        {
            if (!hasRealSnapshot)
            {
                return;
            }
            points = realSnapshot;
        }
        tree.EarnedWeaverPoints = (ushort)ClampPoints(points);
        overrideApplied = false;
        hasRealSnapshot = false;
    }

    static void RefreshOpenUi()
    {
        foreach (FactionRankUIWeaver ui in Object.FindObjectsOfType<FactionRankUIWeaver>())
        {
            if (ui != null)
            {
                ui.UpdateUnspentPointsRoot();
            }
        }
    }

    [HarmonyPatch(typeof(TheWeaver), "get_EarnedWeaverPoints")]
    public class TheWeaver_GetEarned
    {
        [HarmonyPostfix]
        static void Postfix(ref int __result)
        {
            if (!Active())
            {
                return;
            }
            __result = TargetPoints();
        }
    }

    [HarmonyPatch(typeof(TheWeaver), "get_MaxWeaverPoints")]
    public class TheWeaver_GetMax
    {
        [HarmonyPostfix]
        static void Postfix(ref int __result)
        {
            if (readingMax || !Active())
            {
                return;
            }
            int points = TargetPoints();
            if (points > __result)
            {
                __result = points;
            }
        }
    }

    [HarmonyPatch(typeof(LocalTreeData.WeaverTreeData), "getUnspentPoints")]
    public class WeaverTreeData_GetUnspent
    {
        [HarmonyPrefix]
        static void Prefix(LocalTreeData.WeaverTreeData __instance)
        {
            if (!Active())
            {
                return;
            }
            WritePoints(__instance);
        }
    }

    [HarmonyPatch(typeof(FactionRankUIWeaver), "OpenWeaverTree")]
    public class FactionRankUIWeaver_Open
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            ApplyToPlayer();
        }
    }

    [HarmonyPatch(typeof(FactionRankUIWeaver), "Awake")]
    public class FactionRankUIWeaver_Awake
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            ApplyToPlayer();
        }
    }

    [HarmonyPatch(typeof(TheWeaver), nameof(TheWeaver.CompleteWovenEcho))]
    public class TheWeaver_CompleteWovenEcho
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            if (Active())
            {
                ApplyToPlayer();
                return;
            }
            if (!usedOverride || writing)
            {
                return;
            }
            if (
                Refs_Manager.player_treedata.IsNullOrDestroyed()
                || Refs_Manager.player_treedata.weaverTree == null
            )
            {
                return;
            }
            writing = true;
            try
            {
                RestoreRealPoints(Refs_Manager.player_treedata.weaverTree);
            }
            finally
            {
                writing = false;
            }
            RefreshOpenUi();
        }
    }
}
