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

        [Header("Air / Gravity")]
        public float gravityScale = 2f;
        [Tooltip("Optional: scale total air time")]
        public float airTimeScale = 1f;

        [Header("Landing")]
        [Tooltip("How strongly pancake is pulled toward pan center when returning")]
        [Range(0f, 1f)]
        public float landingAssistStrength = 0.5f;

        [Header("Scoring (optional)")]
        public int popupPointsPerRotation = 10;

        public float ForceFromCharge(float charge01)
        {
            return Mathf.Lerp(minForce, maxForce, Mathf.Clamp01(charge01));
        }

        public float SpinFromCharge(float charge01)
        {
            return Mathf.Lerp(minSpin, maxSpin, Mathf.Clamp01(charge01));
        }
    }
}
