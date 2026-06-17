using NUnit.Framework;
using IdlePancake.PancakeFlip.ResponsiveCore;

public class OrientationResolverTests
{
    [Test]
    public void Initial_WideAspect_IsLandscape()
    {
        Assert.AreEqual(OrientationMode.Landscape, OrientationResolver.Initial(1.777f));
    }

    [Test]
    public void Initial_TallAspect_IsPortrait()
    {
        Assert.AreEqual(OrientationMode.Portrait, OrientationResolver.Initial(0.5625f));
    }

    [Test]
    public void Initial_ExactSquare_IsLandscape()
    {
        Assert.AreEqual(OrientationMode.Landscape, OrientationResolver.Initial(1.0f));
    }

    [Test]
    public void Resolve_PortraitStaysPortrait_InsideHysteresisBand()
    {
        // 1.02 is above 1.0 but below the enter-landscape threshold (1.05)
        Assert.AreEqual(OrientationMode.Portrait,
            OrientationResolver.Resolve(1.02f, OrientationMode.Portrait));
    }

    [Test]
    public void Resolve_PortraitToLandscape_WhenClearlyWide()
    {
        Assert.AreEqual(OrientationMode.Landscape,
            OrientationResolver.Resolve(1.30f, OrientationMode.Portrait));
    }

    [Test]
    public void Resolve_LandscapeStaysLandscape_InsideHysteresisBand()
    {
        // 0.98 is below 1.0 but above the enter-portrait threshold (0.95)
        Assert.AreEqual(OrientationMode.Landscape,
            OrientationResolver.Resolve(0.98f, OrientationMode.Landscape));
    }

    [Test]
    public void Resolve_LandscapeToPortrait_WhenClearlyTall()
    {
        Assert.AreEqual(OrientationMode.Portrait,
            OrientationResolver.Resolve(0.80f, OrientationMode.Landscape));
    }
}
