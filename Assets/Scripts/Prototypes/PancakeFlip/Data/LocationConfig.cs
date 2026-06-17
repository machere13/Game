using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Location", fileName = "Location")]
    public sealed class LocationConfig : ScriptableObject
    {
        public string displayName = "Локация";
        [Tooltip("Иконка узла на карте (опц.)")]
        public Sprite mapIcon;
        [Tooltip("Фон локации — зарезервировано под арт (пока null)")]
        public Sprite background;

        [Tooltip("Сколько заказов сдать здесь, чтобы открыть следующую локацию")]
        public int ordersToUnlockNext = 5;

        [Tooltip("Что заказывают здешние покупатели (пул заказов локации)")]
        public RecipeConfig[] demandRecipes;

        [Tooltip("Рецепты, добавляемые в книгу при первом прибытии")]
        public RecipeConfig[] unlockRecipes;

        [Tooltip("Ингредиенты, добавляемые в магазин при первом прибытии")]
        public IngredientConfig[] unlockIngredients;

        [Tooltip("Спрайты местных покупателей")]
        public Sprite[] customerSprites;
    }
}
