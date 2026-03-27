using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Pan Upgrade", fileName = "PanUpgrade")]
    public sealed class PanUpgradeConfig : ScriptableObject
    {
        public string displayName = "Апгрейд";
        [TextArea] public string description;
        public Sprite icon;
        public int coinCost = 50;
        public int unlockLevel;

        public enum EffectType
        {
            WiderPerfectZone,
            SlowerOvercook,
            StablerSpin,
            EasierFlip
        }

        public EffectType effectType;
        [Tooltip("Множитель эффекта (например 1.2 = +20%)")]
        public float effectValue = 1.2f;
    }
}
