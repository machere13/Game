using System.Collections.Generic;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeBuild
    {
        public const int MaxSlots = 4;

        readonly List<IngredientConfig> _slots = new();

        public event System.Action OnChanged;

        public IReadOnlyList<IngredientConfig> Slots => _slots;
        public int Count => _slots.Count;
        public bool IsFull => _slots.Count >= MaxSlots;
        public bool IsEmpty => _slots.Count == 0;

        public bool HasDough
        {
            get
            {
                foreach (var s in _slots)
                {
                    if (s != null && s.isDough) return true;
                }
                return false;
            }
        }

        public bool CanCook => HasDough;

        public bool TryAdd(IngredientConfig ingredient)
        {
            if (ingredient == null) return false;
            if (_slots.Count >= MaxSlots) return false;
            _slots.Add(ingredient);
            OnChanged?.Invoke();
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _slots.Count) return false;
            _slots.RemoveAt(index);
            OnChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            if (_slots.Count == 0) return;
            _slots.Clear();
            OnChanged?.Invoke();
        }

        public Dictionary<IngredientConfig, int> ToAggregate()
        {
            var map = new Dictionary<IngredientConfig, int>();
            foreach (var s in _slots)
            {
                if (s == null) continue;
                map.TryGetValue(s, out int n);
                map[s] = n + 1;
            }
            return map;
        }
    }
}
