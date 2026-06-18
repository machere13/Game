using System;

namespace IdlePancake.PancakeFlip.JuiceCore
{
    /// <summary>Pure easing/envelope curves for juice. No UnityEngine dependency.</summary>
    public static class JuiceCurves
    {
        /// <summary>Standard ease-out-back: 0..1 with a slight overshoot above 1 near the end.</summary>
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float p = t - 1f;
            return 1f + c3 * p * p * p + c1 * p * p;
        }

        /// <summary>Punch envelope: 0 at t=0 and t=1, peak of 1 at t=0.5 (sine half-wave).</summary>
        public static float PunchScale01(float t)
        {
            if (t <= 0f || t >= 1f) return 0f;
            return (float)Math.Sin(Math.PI * t);
        }
    }
}
