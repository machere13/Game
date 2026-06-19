using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Pan Tier", fileName = "PanTier")]
    public sealed class PanTierConfig : ScriptableObject
    {
        public string displayName = "Сковорода";
        [TextArea] public string description;
        public Sprite icon;
        [Tooltip("Спрайт передней части сковороды в кухне (мир).")]
        public Sprite panFront;

        [Tooltip("0 = стартовая, всегда в коллекции при новой игре.")]
        public bool isStarter;

        public int coinCost = 80;
        public int unlockLevel;

        [Header("Базовые множители этой сковороды (1 = без бонуса)")]
        [Tooltip("Шире зона идеальной прожарки.")]
        public float widerPerfectZone = 1f;
        [Tooltip("Выше порог пережарки / медленнее штраф.")]
        public float slowerOvercook = 1f;
        [Tooltip("Стабильнее вращение (чем больше, тем меньше крутизна).")]
        public float stablerSpin = 1f;
        [Tooltip("Сильнее толчок при том же заряде.")]
        public float easierFlip = 1f;

        public float Multiplier(PanUpgradeConfig.EffectType t)
        {
            return t switch
            {
                PanUpgradeConfig.EffectType.WiderPerfectZone => widerPerfectZone,
                PanUpgradeConfig.EffectType.SlowerOvercook => slowerOvercook,
                PanUpgradeConfig.EffectType.StablerSpin => stablerSpin,
                PanUpgradeConfig.EffectType.EasierFlip => easierFlip,
                _ => 1f
            };
        }
    }
}
