using NUnit.Framework;
using IdlePancake.PancakeFlip.TutorialCore;

public class TutorialProgressTests
{
    [Test]
    public void New_StartsAtZero_NotComplete()
    {
        var p = new TutorialProgress(6);
        Assert.AreEqual(6, p.StepCount);
        Assert.AreEqual(0, p.CurrentIndex);
        Assert.IsFalse(p.IsComplete);
    }

    [Test]
    public void Advance_ToLastStep_NotCompleteYet()
    {
        var p = new TutorialProgress(6);
        for (int i = 0; i < 5; i++) p.Advance();
        Assert.AreEqual(5, p.CurrentIndex);
        Assert.IsFalse(p.IsComplete);
    }

    [Test]
    public void Advance_PastLastStep_CompletesAndClamps()
    {
        var p = new TutorialProgress(6);
        for (int i = 0; i < 6; i++) p.Advance();
        Assert.IsTrue(p.IsComplete);
        Assert.AreEqual(6, p.CurrentIndex);
        p.Advance();
        Assert.AreEqual(6, p.CurrentIndex);
    }

    [Test]
    public void EmptySequence_IsCompleteImmediately()
    {
        var p = new TutorialProgress(0);
        Assert.IsTrue(p.IsComplete);
    }
}
