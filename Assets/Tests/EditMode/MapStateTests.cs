using NUnit.Framework;
using IdlePancake.PancakeFlip.MapCore;

public class MapStateTests
{
    static MapState Make() => new MapState(new[] { 5, 8, 10 });

    [Test]
    public void New_StartsAtFirstLocation_OnlyFirstUnlocked()
    {
        var m = Make();
        Assert.AreEqual(3, m.LocationCount);
        Assert.AreEqual(0, m.CurrentIndex);
        Assert.AreEqual(0, m.HighestUnlockedIndex);
    }

    [Test]
    public void RecordOrders_BelowThreshold_DoesNotUnlock()
    {
        var m = Make();
        for (int i = 0; i < 4; i++)
            Assert.IsFalse(m.RecordOrderCompleted());
        Assert.AreEqual(0, m.HighestUnlockedIndex);
    }

    [Test]
    public void RecordOrders_AtThreshold_UnlocksNextOnce()
    {
        var m = Make();
        bool unlocked = false;
        for (int i = 0; i < 5; i++) unlocked = m.RecordOrderCompleted();
        Assert.IsTrue(unlocked);
        Assert.AreEqual(1, m.HighestUnlockedIndex);
        Assert.IsFalse(m.RecordOrderCompleted());
        Assert.AreEqual(1, m.HighestUnlockedIndex);
    }

    [Test]
    public void CanTravelTo_RespectsUnlockAndBounds()
    {
        var m = Make();
        Assert.IsTrue(m.CanTravelTo(0));
        Assert.IsFalse(m.CanTravelTo(1));
        Assert.IsFalse(m.CanTravelTo(-1));
        Assert.IsFalse(m.CanTravelTo(3));
        for (int i = 0; i < 5; i++) m.RecordOrderCompleted();
        Assert.IsTrue(m.CanTravelTo(1));
    }

    [Test]
    public void TravelTo_UnlockedLocation_MovesCurrent()
    {
        var m = Make();
        for (int i = 0; i < 5; i++) m.RecordOrderCompleted();
        Assert.IsTrue(m.TravelTo(1));
        Assert.AreEqual(1, m.CurrentIndex);
        Assert.IsFalse(m.TravelTo(2));
        Assert.AreEqual(1, m.CurrentIndex);
    }

    [Test]
    public void ShouldApplyUnlock_OncePerLocation()
    {
        var m = Make();
        Assert.IsTrue(m.ShouldApplyUnlock(0));
        m.MarkUnlockApplied(0);
        Assert.IsFalse(m.ShouldApplyUnlock(0));
    }

    [Test]
    public void ReturningToEarlierLocation_DoesNotUnlockAhead()
    {
        var m = Make();
        for (int i = 0; i < 5; i++) m.RecordOrderCompleted();
        m.TravelTo(1);
        m.TravelTo(0);
        for (int i = 0; i < 6; i++) Assert.IsFalse(m.RecordOrderCompleted());
        Assert.AreEqual(1, m.HighestUnlockedIndex);
    }

    [Test]
    public void LastLocation_NeverUnlocksBeyond()
    {
        var m = Make();
        for (int i = 0; i < 5; i++) m.RecordOrderCompleted();
        m.TravelTo(1);
        for (int i = 0; i < 8; i++) m.RecordOrderCompleted();
        m.TravelTo(2);
        for (int i = 0; i < 20; i++) Assert.IsFalse(m.RecordOrderCompleted());
        Assert.AreEqual(2, m.HighestUnlockedIndex);
    }
}
