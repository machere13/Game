namespace IdlePancake.PancakeFlip.ResponsiveCore
{
    /// <summary>
    /// Pure decision: screen aspect (width / height) -> orientation mode.
    /// Uses a hysteresis band around 1.0 so a window dragged near-square does not flicker.
    /// </summary>
    public static class OrientationResolver
    {
        public const float EnterLandscapeAbove = 1.05f;
        public const float EnterPortraitBelow = 0.95f;

        public static OrientationMode Initial(float aspect)
        {
            return aspect >= 1.0f ? OrientationMode.Landscape : OrientationMode.Portrait;
        }

        public static OrientationMode Resolve(float aspect, OrientationMode current)
        {
            if (current == OrientationMode.Portrait)
                return aspect >= EnterLandscapeAbove ? OrientationMode.Landscape : OrientationMode.Portrait;

            // current == Landscape
            return aspect <= EnterPortraitBelow ? OrientationMode.Portrait : OrientationMode.Landscape;
        }
    }
}
