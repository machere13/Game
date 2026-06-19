using System;

namespace IdlePancake.PancakeFlip.MapCore
{
    public enum CityState { Owned, Buyable, Locked }

    /// <summary>
    /// Pure city-ownership + level-gate state for the world map. No UnityEngine dependency.
    /// A city is Owned (bought), Buyable (player level >= required, not bought), or Locked.
    /// </summary>
    public sealed class MapState
    {
        readonly int[] _requiredLevels;
        readonly bool[] _owned;

        public int LocationCount { get; }
        public int CurrentIndex { get; private set; }

        public event Action OnChanged;

        public MapState(int[] requiredLevels, int startOwnedIndex = 0)
        {
            if (requiredLevels == null || requiredLevels.Length == 0)
                throw new ArgumentException("requiredLevels must be non-empty");
            LocationCount = requiredLevels.Length;
            _requiredLevels = (int[])requiredLevels.Clone();
            _owned = new bool[LocationCount];
            if (startOwnedIndex >= 0 && startOwnedIndex < LocationCount)
            {
                _owned[startOwnedIndex] = true;
                CurrentIndex = startOwnedIndex;
            }
            else CurrentIndex = 0;
        }

        bool InRange(int i) => i >= 0 && i < LocationCount;

        public bool IsOwned(int i) => InRange(i) && _owned[i];
        public int RequiredLevel(int i) => InRange(i) ? _requiredLevels[i] : int.MaxValue;

        public CityState StateOf(int i, int playerLevel)
        {
            if (!InRange(i)) return CityState.Locked;
            if (_owned[i]) return CityState.Owned;
            return playerLevel >= _requiredLevels[i] ? CityState.Buyable : CityState.Locked;
        }

        public bool CanEnter(int i) => IsOwned(i);

        public bool CanBuy(int i, int playerLevel) =>
            InRange(i) && !_owned[i] && playerLevel >= _requiredLevels[i];

        public void MarkOwned(int i)
        {
            if (InRange(i) && !_owned[i]) { _owned[i] = true; OnChanged?.Invoke(); }
        }

        public bool SetCurrent(int i)
        {
            if (!CanEnter(i)) return false;
            CurrentIndex = i;
            OnChanged?.Invoke();
            return true;
        }
    }
}
