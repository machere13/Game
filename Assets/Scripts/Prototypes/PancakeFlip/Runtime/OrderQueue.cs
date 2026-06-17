using System.Collections.Generic;
using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class OrderQueue
    {
        readonly List<Order> _visible = new();
        readonly List<RecipeConfig> _source = new();
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
            {
                foreach (var r in availableRecipes)
                {
                    if (r != null) _source.Add(r);
                }
            }
            _pool.AddRange(_source);
            Refill();
        }

        public void DismissOrder(Order order)
        {
            _visible.Remove(order);
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
            while (_visible.Count < _maxVisible)
            {
                if (_pool.Count == 0)
                {
                    if (_source.Count == 0) return;
                    _pool.AddRange(_source);
                }
                int idx = Random.Range(0, _pool.Count);
                var recipe = _pool[idx];
                _pool.RemoveAt(idx);
                _visible.Add(new Order(recipe, Random.Range(0, _personCount)));
            }
        }

        public void AddRecipesToPool(RecipeConfig[] recipes)
        {
            if (recipes == null) return;
            bool added = false;
            foreach (var r in recipes)
            {
                if (r == null || _source.Contains(r)) continue;
                _source.Add(r);
                _pool.Add(r);
                added = true;
            }
            if (!added) return;
            Refill();
            OnChanged?.Invoke();
        }

        public void SetSource(RecipeConfig[] recipes)
        {
            _source.Clear();
            _pool.Clear();
            _visible.Clear();
            if (recipes != null)
            {
                foreach (var r in recipes)
                    if (r != null) _source.Add(r);
            }
            _pool.AddRange(_source);
            Refill();
            OnChanged?.Invoke();
        }
    }
}
