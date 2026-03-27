using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Recipe", fileName = "Recipe")]
    public sealed class RecipeConfig : ScriptableObject
    {
        public string displayName = "Блин";
        public Sprite icon;
        [Tooltip("Уровень открытия")]
        public int unlockLevel;

        [System.Serializable]
        public struct IngredientSlot
        {
            public IngredientConfig ingredient;
            public int amount;
        }

        public IngredientSlot[] ingredients;

        [Header("Reward")]
        public int rewardCoins = 10;
        public int rewardXp = 20;
    }
}
