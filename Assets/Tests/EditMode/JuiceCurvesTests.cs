using NUnit.Framework;
using IdlePancake.PancakeFlip.JuiceCore;

public class JuiceCurvesTests
{
    [Test]
    public void EaseOutBack_Endpoints()
    {
        Assert.AreEqual(0f, JuiceCurves.EaseOutBack(0f), 0.0001f);
        Assert.AreEqual(1f, JuiceCurves.EaseOutBack(1f), 0.0001f);
    }

    [Test]
    public void EaseOutBack_OvershootsAboveOne()
    {
        // ease-out-back rises above 1 before settling
        Assert.Greater(JuiceCurves.EaseOutBack(0.85f), 1f);
    }

    [Test]
    public void PunchScale01_ZeroAtEnds()
    {
        Assert.AreEqual(0f, JuiceCurves.PunchScale01(0f), 0.0001f);
        Assert.AreEqual(0f, JuiceCurves.PunchScale01(1f), 0.0001f);
    }

    [Test]
    public void PunchScale01_PeaksInMiddle()
    {
        Assert.AreEqual(1f, JuiceCurves.PunchScale01(0.5f), 0.0001f);
        Assert.Greater(JuiceCurves.PunchScale01(0.5f), JuiceCurves.PunchScale01(0.1f));
    }
}
