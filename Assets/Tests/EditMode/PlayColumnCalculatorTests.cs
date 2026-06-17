using NUnit.Framework;
using IdlePancake.PancakeFlip.ResponsiveCore;

public class PlayColumnCalculatorTests
{
    const float Ortho = 5f;             // camera half-height -> camH = 10
    const float ColumnAspect = 9f / 16f; // 0.5625
    const float PortraitAspect = 9f / 16f;
    const float LandscapeAspect = 16f / 9f;

    [Test]
    public void Portrait_ColumnFillsFullWidth()
    {
        var m = PlayColumnCalculator.Compute(Ortho, PortraitAspect, ColumnAspect);
        Assert.AreEqual(1.0f, m.ScreenFractionWidth, 0.001f);
        Assert.AreEqual(0.0f, m.SideMarginFraction, 0.001f);
    }

    [Test]
    public void Landscape_ColumnIsNarrowerThanScreen()
    {
        var m = PlayColumnCalculator.Compute(Ortho, LandscapeAspect, ColumnAspect);
        Assert.Less(m.ScreenFractionWidth, 1.0f);
        Assert.Greater(m.SideMarginFraction, 0.0f);
    }

    [Test]
    public void ColumnWorldWidth_IsSame_InPortraitAndLandscape()
    {
        var p = PlayColumnCalculator.Compute(Ortho, PortraitAspect, ColumnAspect);
        var l = PlayColumnCalculator.Compute(Ortho, LandscapeAspect, ColumnAspect);
        Assert.AreEqual(p.WorldWidth, l.WorldWidth, 0.001f);
    }

    [Test]
    public void ColumnWorldWidth_EqualsHeightTimesAspect()
    {
        var m = PlayColumnCalculator.Compute(Ortho, LandscapeAspect, ColumnAspect);
        // camH = 10, column height == camH, width = height * aspect
        Assert.AreEqual(10f, m.WorldHeight, 0.001f);
        Assert.AreEqual(10f * ColumnAspect, m.WorldWidth, 0.001f);
    }

    [Test]
    public void SideMargin_IsHalfOfRemainingWidth()
    {
        var m = PlayColumnCalculator.Compute(Ortho, LandscapeAspect, ColumnAspect);
        Assert.AreEqual((1f - m.ScreenFractionWidth) * 0.5f, m.SideMarginFraction, 0.001f);
    }
}
