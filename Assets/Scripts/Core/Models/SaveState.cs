using System;
using System.Collections.Generic;

namespace IdlePancake.Core.Models
{
    public sealed class SaveState
    {
        public int Version { get; init; }
        public string ProfileId { get; init; } = "default";
        public DateTime LastSeenUtc { get; init; }
        public DateTime CreatedUtc { get; init; }
        public DateTime UpdatedUtc { get; init; }
        public PlayerState Player { get; init; } = new();
        public KitchenState Kitchen { get; init; } = new();
        public PrestigeState Prestige { get; init; } = new();
        public MetaState Meta { get; init; } = new();
    }

    public sealed class PlayerState
    {
        public CurrencyWallet Wallet { get; init; } = new();
        public long TotalCoinsEarnedLifetime { get; init; }
        public int TotalPrestigesLifetime { get; init; }
    }

    public sealed class CurrencyWallet
    {
        public long Coins { get; init; }
        public long PancakeToken { get; init; }
    }

    public sealed class KitchenState
    {
        public long UncollectedCoins { get; init; }
        public IReadOnlyList<RecipeProgress> Recipes { get; init; } = Array.Empty<RecipeProgress>();
        public IReadOnlyList<UpgradeProgress> Upgrades { get; init; } = Array.Empty<UpgradeProgress>();
    }

    public sealed class RecipeProgress
    {
        public string RecipeId { get; init; } = string.Empty;
        public bool IsUnlocked { get; init; }
        public int Level { get; init; }
    }

    public sealed class UpgradeProgress
    {
        public string UpgradeId { get; init; } = string.Empty;
        public int Level { get; init; }
    }

    public sealed class PrestigeState
    {
        public int Rank { get; init; }
        public IReadOnlyList<UpgradeProgress> MetaUpgrades { get; init; } = Array.Empty<UpgradeProgress>();
    }

    public sealed class MetaState
    {
        public IReadOnlyDictionary<string, bool> TutorialFlags { get; init; } = new Dictionary<string, bool>();
        public IReadOnlyList<string> Achievements { get; init; } = Array.Empty<string>();
    }
}
