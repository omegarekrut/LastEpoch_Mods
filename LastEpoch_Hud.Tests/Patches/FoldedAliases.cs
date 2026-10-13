namespace LastEpoch_Hud.Tests.Patches;

/// <summary>Folded partners that are the same method under another name, so hooking one is meant to hook both.</summary>
internal static class FoldedAliases
{
    // TooltipItemManager.OpenItemTooltip and OpenTooltip share parameters and one body (0x1492650);
    // its only caller is TooltipSystem.OpenItemTooltip, so the postfix runs exactly when intended.
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Known =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["TooltipItemManager::OpenTooltip"] = ["TooltipItemManager::OpenItemTooltip"],
        };

    /// <summary>The partners that are not accepted aliases of the target, order kept.</summary>
    public static IReadOnlyList<string> Unexpected(
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliases,
        string targetKey,
        IReadOnlyList<string> others
    )
    {
        if (!aliases.TryGetValue(targetKey, out IReadOnlyList<string> accepted))
        {
            return others;
        }

        return others.Where(other => !accepted.Contains(other)).ToList();
    }

    /// <summary>Accepted aliases that no longer share the target's code.</summary>
    public static IReadOnlyList<string> Stale(
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliases,
        string targetKey,
        IReadOnlyList<string> others
    )
    {
        if (!aliases.TryGetValue(targetKey, out IReadOnlyList<string> accepted))
        {
            return [];
        }

        return accepted.Where(alias => !others.Contains(alias)).ToList();
    }
}
