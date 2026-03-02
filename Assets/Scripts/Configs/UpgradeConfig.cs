using UnityEngine;

namespace IdlePancake.Configs
{
    [CreateAssetMenu(menuName = "IdlePancake/Configs/Upgrade", fileName = "UpgradeConfig")]
    public sealed class UpgradeConfig : ScriptableObject
    {
        [field: SerializeField] public string UpgradeId { get; private set; } = string.Empty;
        [field: SerializeField] public string DisplayName { get; private set; } = string.Empty;
        [field: SerializeField] public long BaseCost { get; private set; }
        [field: SerializeField] public float GrowthRate { get; private set; } = 1.15f;
        [field: SerializeField] public int MaxLevel { get; private set; } = 100;
    }
}
