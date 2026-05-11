using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Pan Stat Track", fileName = "PanStatTrack")]
    public sealed class PanStatTrackConfig : ScriptableObject
    {
        public string displayName = "Характеристика";
        [TextArea] public string description;
        public Sprite icon;
        public PanUpgradeConfig.EffectType effectType;
        public int unlockLevel;

        [Tooltip("Монеты за шаг 1; за шаг N считается как base × N.")]
        public int coinCostBase = 25;

        public int CostForNextLevel(int currentLevel)
        {
            return Mathf.Max(1, Mathf.RoundToInt(coinCostBase * (currentLevel + 1)));
        }
    }
}
