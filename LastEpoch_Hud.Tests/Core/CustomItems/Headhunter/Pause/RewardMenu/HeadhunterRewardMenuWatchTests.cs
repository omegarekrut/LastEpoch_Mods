using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause.RewardMenu;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause.RewardMenu;

public sealed class HeadhunterRewardMenuWatchTests
{
    private const double Opened = 10;

    private readonly HeadhunterRewardMenuWatch _watch = new();

    [Fact]
    public void Start_Active()
    {
        StartA();

        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_NotStarted_False()
    {
        bool ended = _watch.TryEnd(HeadhunterRewardMenuState.Missing, Opened, out _);

        Assert.False(ended);
    }

    [Fact]
    public void TryEnd_OpenBeforeCap_False()
    {
        StartA();

        bool ended = _watch.TryEnd(HeadhunterRewardMenuState.Open, 309.9, out _);

        Assert.False(ended);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_OpenAtCap_ByCap()
    {
        StartA();

        bool ended = _watch.TryEnd(
            HeadhunterRewardMenuState.Open,
            310,
            out HeadhunterRewardMenuStop stop
        );

        Assert.True(ended);
        Assert.Equal(HeadhunterRewardMenuEnd.Cap, stop.By);
        Assert.Equal(300, stop.HeldSeconds, 6);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_HiddenWithinGrace_False()
    {
        StartA();

        bool ended = _watch.TryEnd(HeadhunterRewardMenuState.Hidden, 10.5, out _);

        Assert.False(ended);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_HiddenAtGrace_ByHidden()
    {
        StartA();

        bool ended = _watch.TryEnd(
            HeadhunterRewardMenuState.Hidden,
            11,
            out HeadhunterRewardMenuStop stop
        );

        Assert.True(ended);
        Assert.Equal(HeadhunterRewardMenuEnd.Hidden, stop.By);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_Missing_ByMissingAtOnce()
    {
        StartA();

        bool ended = _watch.TryEnd(
            HeadhunterRewardMenuState.Missing,
            10.25,
            out HeadhunterRewardMenuStop stop
        );

        Assert.True(ended);
        Assert.Equal(HeadhunterRewardMenuEnd.Missing, stop.By);
        Assert.Equal(0.25, stop.HeldSeconds, 6);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryClose_SameId_ByClosed()
    {
        StartA();

        bool ended = _watch.TryClose(7, 14, out HeadhunterRewardMenuStop stop);

        Assert.True(ended);
        Assert.Equal(
            new HeadhunterRewardMenuStop(
                new HeadhunterRewardMenu(7, "FakePanelA", Opened),
                4,
                HeadhunterRewardMenuEnd.Closed
            ),
            stop
        );
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryClose_OtherId_False()
    {
        StartA();

        bool ended = _watch.TryClose(8, 14, out _);

        Assert.False(ended);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryClose_NotStarted_False()
    {
        Assert.False(_watch.TryClose(7, 14, out _));
    }

    [Fact]
    public void Start_Twice_LatestWins()
    {
        StartA();
        _watch.Start(8, "FakePanelB", 12);

        Assert.False(_watch.TryClose(7, 14, out _));
        bool ended = _watch.TryClose(8, 15, out HeadhunterRewardMenuStop stop);

        Assert.True(ended);
        Assert.Equal(3, stop.HeldSeconds, 6);
        Assert.Equal("FakePanelB", stop.Menu.Panel);
    }

    [Fact]
    public void TryPeek_Active_ReportsWithoutEnding()
    {
        StartA();

        bool peeked = _watch.TryPeek(
            13,
            HeadhunterRewardMenuEnd.Scene,
            out HeadhunterRewardMenuStop stop
        );

        Assert.True(peeked);
        Assert.Equal(HeadhunterRewardMenuEnd.Scene, stop.By);
        Assert.Equal(3, stop.HeldSeconds, 6);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryPeek_NotStarted_False()
    {
        Assert.False(_watch.TryPeek(13, HeadhunterRewardMenuEnd.Scene, out _));
    }

    [Fact]
    public void Reset_Inactive()
    {
        StartA();

        _watch.Reset();

        Assert.False(_watch.IsActive);
        Assert.False(_watch.TryClose(7, 14, out _));
    }

    private void StartA()
    {
        _watch.Start(7, "FakePanelA", Opened);
    }
}
