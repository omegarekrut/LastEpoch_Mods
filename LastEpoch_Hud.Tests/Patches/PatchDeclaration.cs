using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>One [HarmonyPatch(typeof(T), "Method", argTypes?)] found in the built mod.</summary>
internal sealed record PatchDeclaration(
    string PatchClass,
    TypeReference Target,
    string Method,
    IReadOnlyList<string> ArgumentTypes
)
{
    public string Id => $"patch:{PatchClass}";

    public string FoldedId => $"folded:{PatchClass}";
}
