using System.Collections.Generic;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PanUpgradeState
    {
        readonly HashSet<PanUpgradeConfig> _purchased = new();

        public event System.Action OnChanged;

        public bool IsOwned(PanUpgradeConfig upgrade) => _purchased.Contains(upgrade);

        public void Purchase(PanUpgradeConfig upgrade)
        {
            _purchased.Add(upgrade);
            OnChanged?.Invoke();
        }

        public float GetMultiplier(PanUpgradeConfig.EffectType type)
        {
            float m = 1f;
            foreach (var u in _purchased)
                if (u.effectType == type)
                    m *= u.effectValue;
            return m;
        }
    }
}
