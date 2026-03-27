using System.Collections.Generic;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class Inventory
    {
        readonly Dictionary<IngredientConfig, int> _stock = new();

        public event System.Action OnChanged;

        public int GetAmount(IngredientConfig ingredient)
        {
            if (ingredient == null) return 0;
            if (ingredient.infinite) return 999;
            return _stock.TryGetValue(ingredient, out int n) ? n : 0;
        }

        public void Add(IngredientConfig ingredient, int amount)
        {
            if (ingredient == null || ingredient.infinite) return;
            _stock.TryGetValue(ingredient, out int cur);
            _stock[ingredient] = cur + amount;
            OnChanged?.Invoke();
        }

        public bool HasIngredients(RecipeConfig recipe)
        {
            if (recipe == null || recipe.ingredients == null) return true;
            foreach (var slot in recipe.ingredients)
            {
                if (slot.ingredient == null) continue;
                if (GetAmount(slot.ingredient) < slot.amount) return false;
            }
            return true;
        }

        public bool Consume(RecipeConfig recipe)
        {
            if (!HasIngredients(recipe)) return false;
            if (recipe.ingredients == null) return true;
            foreach (var slot in recipe.ingredients)
            {
                if (slot.ingredient == null || slot.ingredient.infinite) continue;
                _stock.TryGetValue(slot.ingredient, out int cur);
                _stock[slot.ingredient] = cur - slot.amount;
            }
            OnChanged?.Invoke();
            return true;
        }
    }
}
