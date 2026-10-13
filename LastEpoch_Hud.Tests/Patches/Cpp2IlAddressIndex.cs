using System.Globalization;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>Method name to GameAssembly.dll file offsets, read from cpp2il output.</summary>
internal sealed class Cpp2IlAddressIndex
{
    // Methods without an address (abstract, stripped) stay known, with no offsets.
    internal const long NoOffset = -1;

    private const string AddressAttribute = "AddressAttribute";
    private const string OffsetField = "Offset";

    private readonly Dictionary<string, List<long>> _offsets;
    private readonly Dictionary<long, HashSet<string>> _methodsAt;

    private Cpp2IlAddressIndex(
        Dictionary<string, List<long>> offsets,
        Dictionary<long, HashSet<string>> methodsAt
    )
    {
        _offsets = offsets;
        _methodsAt = methodsAt;
    }

    public static Cpp2IlAddressIndex Load(string cpp2IlOutDir)
    {
        var methods = new List<(string Type, string Method, long Offset)>();
        foreach (string dll in Directory.EnumerateFiles(cpp2IlOutDir, "*.dll"))
        {
            using var module = ModuleDefinition.ReadModule(dll);
            methods.AddRange(module.GetTypes().SelectMany(MethodsOf));
        }
        return FromMethods(methods);
    }

    public static Cpp2IlAddressIndex FromMethods(
        IEnumerable<(string Type, string Method, long Offset)> methods
    )
    {
        var offsets = new Dictionary<string, List<long>>();
        var methodsAt = new Dictionary<long, HashSet<string>>();
        foreach ((string type, string method, long offset) in methods)
        {
            string key = Key(type, Cpp2IlNames.ToInteropMethod(method));
            if (!offsets.TryGetValue(key, out List<long> list))
            {
                offsets[key] = list = new List<long>();
            }

            if (offset == NoOffset)
            {
                continue;
            }

            list.Add(offset);
            if (!methodsAt.TryGetValue(offset, out HashSet<string> keys))
            {
                methodsAt[offset] = keys = new HashSet<string>();
            }

            keys.Add(key);
        }
        return new Cpp2IlAddressIndex(offsets, methodsAt);
    }

    public bool TryGetOffsets(
        string interopTypeFullName,
        string method,
        out IReadOnlyList<long> fileOffsets
    )
    {
        string key = Key(Cpp2IlNames.FromInteropType(interopTypeFullName), method);
        bool found = _offsets.TryGetValue(key, out List<long> list);
        fileOffsets = list;
        return found;
    }

    /// <summary>Other methods whose native code starts at any offset of the target.</summary>
    public IReadOnlyList<string> FoldedWith(string interopTypeFullName, string method)
    {
        string key = Key(Cpp2IlNames.FromInteropType(interopTypeFullName), method);
        if (!_offsets.TryGetValue(key, out List<long> targetOffsets))
        {
            return Array.Empty<string>();
        }

        return targetOffsets
            .SelectMany(offset => _methodsAt[offset])
            .Where(other => other != key)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<(string Type, string Method, long Offset)> MethodsOf(
        TypeDefinition type
    ) =>
        type.Methods.SelectMany(method =>
            OffsetsOf(method)
                .DefaultIfEmpty(NoOffset)
                .Select(offset => (type.FullName, method.Name, offset))
        );

    private static IEnumerable<long> OffsetsOf(MethodDefinition method) =>
        method
            .CustomAttributes.Where(a => a.AttributeType.Name == AddressAttribute)
            .SelectMany(a => a.Fields)
            .Where(f => f.Name == OffsetField && f.Argument.Value is string)
            .Select(f =>
                long.Parse(
                    ((string)f.Argument.Value).Replace("0x", ""),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture
                )
            );

    private static string Key(string type, string method) => type + "::" + method;
}
