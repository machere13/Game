using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Ingredient", fileName = "Ingredient")]
    public sealed class IngredientConfig : ScriptableObject
    {
        public string displayName = "Ингредиент";
        public Sprite icon;
        [Tooltip("Стоимость за 1 шт. 0 = бесплатный (тесто)")]
        public int coinCost;
        [Tooltip("Уровень, на котором открывается")]
        public int unlockLevel;
        [Tooltip("Бесконечный ресурс: не тратится при готовке")]
        public bool infinite;
        [Tooltip("Максимум на складе. 0 = без лимита")]
        public int maxStock;
        [Tooltip("Тесто-маркер: основа блина, обязательна в сборке")]
        public bool isDough;
    }
}
