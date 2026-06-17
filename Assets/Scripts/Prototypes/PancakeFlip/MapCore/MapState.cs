using System;

namespace IdlePancake.PancakeFlip.MapCore
{
    /// <summary>
    /// Pure progression logic for the linear location chain. No UnityEngine dependency.
    /// </summary>
    public sealed class MapState
    {
        readonly int[] _ordersToUnlock;
        readonly int[] _completed;
        readonly bool[] _unlockApplied;

        public int LocationCount { get; }
        public int CurrentIndex { get; private set; }
        public int HighestUnlockedIndex { get; private set; }

        public event Action OnChanged;

        public MapState(int[] ordersToUnlockNext)
        {
            if (ordersToUnlockNext == null || ordersToUnlockNext.Length == 0)
                throw new ArgumentException("ordersToUnlockNext must be non-empty");
            LocationCount = ordersToUnlockNext.Length;
            _ordersToUnlock = (int[])ordersToUnlockNext.Clone();
            _completed = new int[LocationCount];
            _unlockApplied = new bool[LocationCount];
            CurrentIndex = 0;
            HighestUnlockedIndex = 0;
        }

        public bool RecordOrderCompleted()
        {
            _completed[CurrentIndex]++;
            bool unlockedNew = false;
            if (HighestUnlockedIndex == CurrentIndex
                && CurrentIndex + 1 < LocationCount
                && _completed[CurrentIndex] >= _ordersToUnlock[CurrentIndex])
            {
                HighestUnlockedIndex = CurrentIndex + 1;
                unlockedNew = true;
            }
            OnChanged?.Invoke();
            return unlockedNew;
        }

        public bool CanTravelTo(int index) =>
            index >= 0 && index < LocationCount && index <= HighestUnlockedIndex;

        public bool TravelTo(int index)
        {
            if (!CanTravelTo(index)) return false;
            CurrentIndex = index;
            OnChanged?.Invoke();
            return true;
        }

        public bool ShouldApplyUnlock(int index)
        {
            if (index < 0 || index >= LocationCount) return false;
            return !_unlockApplied[index];
        }

        public void MarkUnlockApplied(int index)
        {
            if (index >= 0 && index < LocationCount) _unlockApplied[index] = true;
        }

        public int OrdersInCurrent() => _completed[CurrentIndex];
        public int OrdersToUnlockCurrent() => _ordersToUnlock[CurrentIndex];
    }
}
