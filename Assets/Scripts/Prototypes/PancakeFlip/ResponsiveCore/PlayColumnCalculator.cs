namespace IdlePancake.PancakeFlip.ResponsiveCore
{
    /// <summary>Metrics of the centered vertical play column, in world units and screen fractions.</summary>
    public readonly struct PlayColumnMetrics
    {
        public readonly float WorldWidth;
        public readonly float WorldHeight;
        public readonly float ScreenFractionWidth; // column width / camera width, clamped to [0,1]
        public readonly float SideMarginFraction;  // (1 - ScreenFractionWidth) / 2

        public PlayColumnMetrics(float worldWidth, float worldHeight, float screenFractionWidth)
        {
            WorldWidth = worldWidth;
            WorldHeight = worldHeight;
            ScreenFractionWidth = screenFractionWidth;
            SideMarginFraction = (1f - screenFractionWidth) * 0.5f;
        }
    }

    /// <summary>
    /// Pure layout math. Given the orthographic half-height, the real screen aspect (w/h),
    /// and the desired column aspect (w/h, e.g. 9/16), computes the centered column.
    /// The column height always equals the camera height; its width is height*columnAspect,
    /// capped to the camera width (so in portrait the column fills the whole width).
    /// </summary>
    public static class PlayColumnCalculator
    {
        public static PlayColumnMetrics Compute(float orthoSize, float screenAspect, float columnAspect)
        {
            float camHeight = orthoSize * 2f;
            float camWidth = camHeight * screenAspect;

            float columnHeight = camHeight;
            float columnWidth = columnHeight * columnAspect;
            if (columnWidth > camWidth)
                columnWidth = camWidth;

            float fraction = camWidth <= 0f ? 1f : columnWidth / camWidth;
            if (fraction > 1f) fraction = 1f;
            if (fraction < 0f) fraction = 0f;

            return new PlayColumnMetrics(columnWidth, columnHeight, fraction);
        }
    }
}
