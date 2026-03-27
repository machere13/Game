using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/PancakeFlip Config", fileName = "PancakeFlipConfig")]
    public sealed class PancakeFlipConfig : ScriptableObject
    {
        [Header("Hold")]
        [Tooltip("Max hold time in seconds; charge caps at this")]
        [Min(0.1f)]
        public float maxHoldTime = 3f;

        [Header("Throw Force")]
        public float minForce = 3f;
        public float maxForce = 12f;

        [Header("Spin")]
        public float minSpin = 360f;
        public float maxSpin = 1440f;
        [Min(0.1f)]
        public float spinMultiplier = 1f;

        [Header("Air / Gravity")]
        public float gravityScale = 2f;
        [Tooltip("Optional: scale total air time")]
        public float airTimeScale = 1f;

        [Header("Landing")]
        [Tooltip("How strongly pancake is pulled toward pan center when returning")]
        [Range(0f, 1f)]
        public float landingAssistStrength = 0.5f;

        [Header("Cooking")]
        [Tooltip("Seconds to fully cook one side (0→1)")]
        public float cookTimePerSide = 8f;
        [Tooltip("Doneness range considered 'perfect' (e.g. 0.4–0.7)")]
        [Range(0f, 1f)] public float perfectMin = 0.4f;
        [Range(0f, 1f)] public float perfectMax = 0.7f;
        [Tooltip("Above this = overcooked")]
        [Range(0f, 1f)] public float overcookedThreshold = 0.85f;

        [Header("Scoring")]
        public int xpPerRotation = 10;

        public float ForceFromCharge(float charge01)
        {
            return Mathf.Lerp(minForce, maxForce, Mathf.Clamp01(charge01));
        }

        public float SpinFromCharge(float charge01)
        {
            return Mathf.Lerp(minSpin, maxSpin, Mathf.Clamp01(charge01)) * spinMultiplier;
        }
    }
}
