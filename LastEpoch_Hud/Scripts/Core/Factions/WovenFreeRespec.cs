using System;

namespace LastEpoch_Hud.Scripts.Core.Factions;

/// <summary>Favor rules for Woven Free Respec.</summary>
public sealed class WovenFreeRespec
{
    private int _depth;

    public bool InRespec => _depth > 0;

    public void Enter()
    {
        _depth++;
    }

    public void Exit()
    {
        _depth = Math.Max(0, _depth - 1);
    }

    public int Spend(int amount, bool enabled)
    {
        return InRespec && enabled ? 0 : amount;
    }

    public static int Cost(int cost, bool enabled)
    {
        return enabled ? 0 : cost;
    }
}
