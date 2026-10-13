using LastEpoch_Hud.Scripts.Core.Factions;
using LastEpoch_Hud.Scripts.ModUI;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven;

public class Faction_Woven_FreeRespe
{
    internal static readonly WovenFreeRespec Rules = new();

    public static bool CanRun() =>
        Scenes.IsGameScene() && ModSettings.FactionsWoven.FreeRespec.Value;
}
