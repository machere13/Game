using NUnit.Framework;
using IdlePancake.PancakeFlip.MapCore;

public class MapStateTests
{
    static MapState Make() => new MapState(new[] { 1, 3, 5 }, 0);

    [Test]
    public void New_StarterOwned_OthersNot()
    {
        var m = Make();
        Assert.AreEqual(3, m.LocationCount);
        Assert.AreEqual(0, m.CurrentIndex);
        Assert.IsTrue(m.IsOwned(0));
        Assert.IsFalse(m.IsOwned(1));
    }

    [Test]
    public void StateOf_DependsOnLevelAndOwnership()
    {
        var m = Make();
        Assert.AreEqual(CityState.Owned, m.StateOf(0, 1));
        Assert.AreEqual(CityState.Locked, m.StateOf(1, 1));
        Assert.AreEqual(CityState.Buyable, m.StateOf(1, 3));
    }

    [Test]
    public void CanBuy_RequiresLevelAndNotOwned()
    {
        var m = Make();
        Assert.IsFalse(m.CanBuy(1, 2));
        Assert.IsTrue(m.CanBuy(1, 3));
        Assert.IsFalse(m.CanBuy(0, 9));
    }

    [Test]
    public void MarkOwned_MakesEnterableAndOwnedState()
    {
        var m = Make();
        Assert.IsFalse(m.CanEnter(1));
        m.MarkOwned(1);
        Assert.IsTrue(m.IsOwned(1));
        Assert.IsTrue(m.CanEnter(1));
        Assert.AreEqual(CityState.Owned, m.StateOf(1, 1));
    }

    [Test]
    public void SetCurrent_OnlyForOwned()
    {
        var m = Make();
        Assert.IsFalse(m.SetCurrent(1));
        Assert.AreEqual(0, m.CurrentIndex);
        m.MarkOwned(1);
        Assert.IsTrue(m.SetCurrent(1));
        Assert.AreEqual(1, m.CurrentIndex);
    }

    [Test]
    public void OutOfRange_IsSafe()
    {
        var m = Make();
        Assert.AreEqual(CityState.Locked, m.StateOf(-1, 99));
        Assert.AreEqual(CityState.Locked, m.StateOf(5, 99));
        Assert.IsFalse(m.CanEnter(9));
        Assert.IsFalse(m.CanBuy(9, 99));
    }
}
