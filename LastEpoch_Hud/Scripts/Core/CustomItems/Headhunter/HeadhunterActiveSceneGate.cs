namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Lets each newly active scene through once, keyed by Unity's scene handle.</summary>
public sealed class HeadhunterActiveSceneGate
{
    private int _handle;

    public bool TryEnter(int handle, string sceneName)
    {
        if (handle == 0)
        {
            return false;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        if (handle == _handle)
        {
            return false;
        }

        _handle = handle;
        return true;
    }
}
