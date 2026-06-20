using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Location", fileName = "Location")]
    public sealed class LocationConfig : ScriptableObject
    {
        public string displayName = "Локация";
        [Tooltip("Иконка узла на карте (опц.)")]
        public Sprite mapIcon;
        [Tooltip("Фон кухни в этом городе")]
        public Sprite background;
        [Tooltip("Спрайт плиты (закрытой) в этом городе")]
        public Sprite stoveClosed;
        [Tooltip("Спрайт плиты (открытой) в этом городе")]
        public Sprite stoveOpen;
        [Tooltip("Нижний прилавок этого города (null = прилавок уже в фоне, не показывать отдельный)")]
        public Sprite bottomPanel;

        [Tooltip("Сколько заказов сдать здесь, чтобы открыть следующую локацию")]
        public int ordersToUnlockNext = 5;

        [Tooltip("Требуемый уровень игрока, чтобы город можно было купить")]
        public int requiredLevel = 1;

        [Tooltip("Стоимость покупки города в монетах (0 = стартовый/бесплатный)")]
        public int cityCost = 0;

        [Tooltip("Позиция маркера на карте, нормализованная 0..1 относительно фона")]
        public Vector2 mapPosition = new Vector2(0.5f, 0.5f);

        [Tooltip("Что заказывают здешние покупатели (пул заказов локации)")]
        public RecipeConfig[] demandRecipes;

        [Tooltip("Рецепты, добавляемые в книгу при первом прибытии")]
        public RecipeConfig[] unlockRecipes;

        [Tooltip("Ингредиенты, добавляемые в магазин при первом прибытии")]
        public IngredientConfig[] unlockIngredients;

        [Tooltip("Спрайты местных покупателей")]
        public Sprite[] customerSprites;

        [Tooltip("Иконки покупателей для карточек заказов (если пусто — берутся общие/customerSprites)")]
        public Sprite[] customerIcons;
    }
}
