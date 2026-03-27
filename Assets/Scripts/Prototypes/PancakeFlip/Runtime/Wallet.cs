using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class Wallet
    {
        public int Coins { get; private set; }
        public int TotalXp { get; private set; }
        public int Level { get; private set; } = 1;
        public int XpInCurrentLevel { get; private set; }

        readonly LevelTableConfig _levels;

        public event System.Action OnChanged;

        public Wallet(LevelTableConfig levels)
        {
            _levels = levels;
        }

        public void AddCoins(int amount)
        {
            Coins += amount;
            OnChanged?.Invoke();
        }

        public bool SpendCoins(int amount)
        {
            if (Coins < amount) return false;
            Coins -= amount;
            OnChanged?.Invoke();
            return true;
        }

        public void AddXp(int amount)
        {
            TotalXp += amount;
            XpInCurrentLevel += amount;

            while (_levels != null && Level < _levels.MaxLevel)
            {
                int needed = _levels.XpForNextLevel(Level);
                if (XpInCurrentLevel < needed) break;
                XpInCurrentLevel -= needed;
                Level++;
            }

            OnChanged?.Invoke();
        }

        public float LevelProgress01()
        {
            if (_levels == null) return 0f;
            int needed = _levels.XpForNextLevel(Level);
            if (needed <= 0 || needed == int.MaxValue) return 1f;
            return Mathf.Clamp01((float)XpInCurrentLevel / needed);
        }
    }
}
