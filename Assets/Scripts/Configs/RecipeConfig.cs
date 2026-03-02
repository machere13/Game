using UnityEngine;

namespace IdlePancake.Configs
{
    [CreateAssetMenu(menuName = "IdlePancake/Configs/Recipe", fileName = "RecipeConfig")]
    public sealed class RecipeConfig : ScriptableObject
    {
        [field: SerializeField] public string RecipeId { get; private set; } = string.Empty;
        [field: SerializeField] public string DisplayName { get; private set; } = string.Empty;
        [field: SerializeField] public int UnlockOrder { get; private set; }
        [field: SerializeField] public long BaseUnlockCost { get; private set; }
        [field: SerializeField] public long BaseRps { get; private set; }
        [field: SerializeField] public float LevelStep { get; private set; } = 0.1f;
    }
}
