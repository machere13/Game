using System;
using System.Collections.Generic;

namespace IdlePancake.Core.Models
{
    public sealed class SaveState
    {
        public int Version;
        public string ProfileId = "default";
        public DateTime LastSeenUtc;
        public DateTime CreatedUtc;
        public DateTime UpdatedUtc;
        public PlayerState Player = new PlayerState();
        public KitchenState Kitchen = new KitchenState();
        public PrestigeState Prestige = new PrestigeState();
        public MetaState Meta = new MetaState();
    }

    public sealed class PlayerState
    {
        public CurrencyWallet Wallet = new CurrencyWallet();
        public long TotalCoinsEarnedLifetime;
        public int TotalPrestigesLifetime;
    }

    public sealed class CurrencyWallet
    {
        public long Coins;
        public long PancakeToken;
    }

    public sealed class KitchenState
    {
        public long UncollectedCoins;
        public List<RecipeProgress> Recipes = new List<RecipeProgress>();
        public List<UpgradeProgress> Upgrades = new List<UpgradeProgress>();
    }

    public sealed class RecipeProgress
    {
        public string RecipeId = string.Empty;
        public bool IsUnlocked;
        public int Level;
    }

    public sealed class UpgradeProgress
    {
        public string UpgradeId = string.Empty;
        public int Level;
    }

    public sealed class PrestigeState
    {
        public int Rank;
        public List<UpgradeProgress> MetaUpgrades = new List<UpgradeProgress>();
    }

    public sealed class MetaState
    {
        public Dictionary<string, bool> TutorialFlags = new Dictionary<string, bool>();
        public List<string> Achievements = new List<string>();
    }
}
