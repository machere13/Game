using UnityEngine;

namespace IdlePancake.Configs
{
    [CreateAssetMenu(menuName = "IdlePancake/Configs/EconomyBalance", fileName = "EconomyBalanceConfig")]
    public sealed class EconomyBalanceConfig : ScriptableObject
    {
        [field: SerializeField] public float OfflineEfficiency { get; private set; } = 0.8f;
        [field: SerializeField] public int OfflineCapSeconds { get; private set; } = 14400;
        [field: SerializeField] public float UnlockGrowth { get; private set; } = 1.3f;
        [field: SerializeField] public float PrestigeK { get; private set; } = 1f;
        [field: SerializeField] public long PrestigeScale { get; private set; } = 100000;
        [field: SerializeField] public long MinPrestigeReward { get; private set; } = 1;
    }
}
