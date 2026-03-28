using System.Collections.Generic;
using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class OrderQueue
    {
        readonly List<Order> _visible = new();
        readonly List<RecipeConfig> _pool = new();
        readonly int _maxVisible;
        readonly int _personCount;

        public IReadOnlyList<Order> Visible => _visible;

        public event System.Action OnChanged;

        public OrderQueue(RecipeConfig[] availableRecipes, int maxVisible = 4, int personCount = 3)
        {
            _maxVisible = maxVisible;
            _personCount = Mathf.Max(1, personCount);
            if (availableRecipes != null)
                _pool.AddRange(availableRecipes);
            Refill();
        }

        public void DismissOrder(Order order)
        {
            if (_visible.Remove(order))
            {
                if (order.Recipe != null)
                    _pool.Add(order.Recipe);
            }
            Refill();
            OnChanged?.Invoke();
        }

        public void CompleteOrder(Order order)
        {
            _visible.Remove(order);
            Refill();
            OnChanged?.Invoke();
        }

        void Refill()
        {
            while (_visible.Count < _maxVisible && _pool.Count > 0)
            {
                int idx = Random.Range(0, _pool.Count);
                var recipe = _pool[idx];
                _pool.RemoveAt(idx);
                _visible.Add(new Order(recipe, Random.Range(0, _personCount)));
            }
        }

        public void AddRecipesToPool(RecipeConfig[] recipes)
        {
            if (recipes == null) return;
            _pool.AddRange(recipes);
            Refill();
            OnChanged?.Invoke();
        }
    }
}
