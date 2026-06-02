using System.Collections.Generic;
using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PanUpgradeState
    {
        public const int StatMaxLevel = 5;
        /// <summary>+4% к множителю характеристики за каждую заполненную ячейку (уровень 5 ≈ +20%).</summary>
        public const float BonusPerStatLevel = 0.04f;

        /// <summary>Число эффектов берётся прямо из enum — не нужно поддерживать вручную.</summary>
        static readonly int EffectTypeCount = System.Enum.GetValues(typeof(PanUpgradeConfig.EffectType)).Length;

        readonly int[] _statLevels = new int[EffectTypeCount];
        readonly HashSet<PanTierConfig> _ownedPans = new();
        PanTierConfig _equippedPan;

        public event System.Action OnChanged;

        public PanTierConfig EquippedPan => _equippedPan;

        public void Initialize(PanTierConfig starterPan)
        {
            _ownedPans.Clear();
            for (int i = 0; i < _statLevels.Length; i++)
                _statLevels[i] = 0;

            if (starterPan != null)
            {
                _ownedPans.Add(starterPan);
                _equippedPan = starterPan;
            }
            else
                _equippedPan = null;
        }

        public int GetStatLevel(PanUpgradeConfig.EffectType type)
        {
            int i = (int)type;
            if (i < 0 || i >= _statLevels.Length) return 0;
            return _statLevels[i];
        }

        public bool IsPanOwned(PanTierConfig tier) => tier != null && _ownedPans.Contains(tier);

        public void EquipPan(PanTierConfig tier)
        {
            if (tier == null || !IsPanOwned(tier)) return;
            _equippedPan = tier;
            OnChanged?.Invoke();
        }

        public bool TryBuyStatStep(PanStatTrackConfig track, Wallet wallet)
        {
            if (track == null || wallet == null) return false;
            if (wallet.Level < track.unlockLevel) return false;
            int L = GetStatLevel(track.effectType);
            if (L >= StatMaxLevel) return false;
            int cost = track.CostForNextLevel(L);
            if (!wallet.SpendCoins(cost)) return false;
            _statLevels[(int)track.effectType] = L + 1;
            OnChanged?.Invoke();
            return true;
        }

        public bool TryBuyPan(PanTierConfig tier, Wallet wallet)
        {
            if (tier == null || wallet == null || tier.isStarter) return false;
            if (IsPanOwned(tier)) return false;
            if (wallet.Level < tier.unlockLevel) return false;
            if (!wallet.SpendCoins(tier.coinCost)) return false;
            _ownedPans.Add(tier);
            OnChanged?.Invoke();
            return true;
        }

        public float GetMultiplier(PanUpgradeConfig.EffectType type)
        {
            float tierPart = _equippedPan != null ? Mathf.Max(0.01f, _equippedPan.Multiplier(type)) : 1f;
            int L = GetStatLevel(type);
            float levelPart = 1f + BonusPerStatLevel * L;
            return tierPart * levelPart;
        }
    }
}
