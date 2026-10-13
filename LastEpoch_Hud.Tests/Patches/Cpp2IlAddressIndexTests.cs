namespace LastEpoch_Hud.Tests.Patches;

public sealed class Cpp2IlAddressIndexTests
{
    [Fact]
    public void FoldedWith_OtherMethodSameOffset_ListsIt()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "get_A", 0x10),
            ("Fake", "get_B", 0x10),
        ]);

        Assert.Equal(["Fake::get_B"], index.FoldedWith("Il2Cpp.Fake", "get_A"));
    }

    [Fact]
    public void FoldedWith_OtherTypeSameOffset_ListsIt()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "get_A", 0x10),
            ("LE.Ns.Other", "get_A", 0x10),
        ]);

        Assert.Equal(["LE.Ns.Other::get_A"], index.FoldedWith("Il2Cpp.Fake", "get_A"));
    }

    [Fact]
    public void FoldedWith_ExplicitInterfaceVariant_Ignored()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "A.IFoo.Bar", 0x10),
            ("Fake", "A_IFoo_Bar", 0x10),
        ]);

        Assert.Empty(index.FoldedWith("Il2Cpp.Fake", "A_IFoo_Bar"));
    }

    [Fact]
    public void FoldedWith_OverloadsSameOffset_Ignored()
    {
        var index = Cpp2IlAddressIndex.FromMethods([("Fake", "Do", 0x10), ("Fake", "Do", 0x10)]);

        Assert.Empty(index.FoldedWith("Il2Cpp.Fake", "Do"));
    }

    [Fact]
    public void FoldedWith_OverloadsEachFolded_ListsOtherOnce()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "Do", 0x10),
            ("Fake", "Do", 0x20),
            ("Fake", "Other", 0x10),
            ("Fake", "Other", 0x20),
            ("Fake", "Third", 0x20),
        ]);

        Assert.Equal(["Fake::Other", "Fake::Third"], index.FoldedWith("Il2Cpp.Fake", "Do"));
    }

    [Fact]
    public void FoldedWith_ManyOthers_SortedOrdinal()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "Do", 0x10),
            ("Fake", "b", 0x10),
            ("Fake", "C", 0x10),
            ("Fake", "a", 0x10),
        ]);

        Assert.Equal(["Fake::C", "Fake::a", "Fake::b"], index.FoldedWith("Il2Cpp.Fake", "Do"));
    }

    [Fact]
    public void FoldedWith_UniqueOffset_Empty()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "get_A", 0x10),
            ("Fake", "get_B", 0x20),
        ]);

        Assert.Empty(index.FoldedWith("Il2Cpp.Fake", "get_A"));
    }

    [Fact]
    public void FoldedWith_UnknownTarget_Empty()
    {
        var index = Cpp2IlAddressIndex.FromMethods([("Fake", "get_A", 0x10)]);

        Assert.Empty(index.FoldedWith("Il2Cpp.Missing", "get_A"));
    }

    [Fact]
    public void FoldedWith_MethodsWithoutAddress_Empty()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "Abstract", Cpp2IlAddressIndex.NoOffset),
            ("Fake", "Other", Cpp2IlAddressIndex.NoOffset),
        ]);

        Assert.Empty(index.FoldedWith("Il2Cpp.Fake", "Abstract"));
    }

    [Fact]
    public void TargetKey_StripsInteropPrefix()
    {
        Assert.Equal("Fake::get_A", Cpp2IlAddressIndex.TargetKey("Il2Cpp.Fake", "get_A"));
        Assert.Equal("LE.Ns.Fake::Do", Cpp2IlAddressIndex.TargetKey("Il2CppLE.Ns.Fake", "Do"));
    }

    [Fact]
    public void FoldedWithKey_MatchesFoldedWith()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "get_A", 0x10),
            ("Fake", "get_B", 0x10),
        ]);

        Assert.Equal(["Fake::get_B"], index.FoldedWithKey("Fake::get_A"));
        Assert.Equal(index.FoldedWith("Il2Cpp.Fake", "get_A"), index.FoldedWithKey("Fake::get_A"));
    }

    [Fact]
    public void TryGetOffsets_MethodWithoutAddress_FoundWithNoOffsets()
    {
        var index = Cpp2IlAddressIndex.FromMethods([
            ("Fake", "Abstract", Cpp2IlAddressIndex.NoOffset),
        ]);

        bool found = index.TryGetOffsets(
            "Il2Cpp.Fake",
            "Abstract",
            out IReadOnlyList<long> offsets
        );

        Assert.True(found);
        Assert.Empty(offsets);
    }

    [Fact]
    public void TryGetOffsets_Overloads_ReturnsAllOffsets()
    {
        var index = Cpp2IlAddressIndex.FromMethods([("Fake", "Do", 0x10), ("Fake", "Do", 0x20)]);

        index.TryGetOffsets("Il2Cpp.Fake", "Do", out IReadOnlyList<long> offsets);

        Assert.Equal([0x10L, 0x20L], offsets);
    }
}
