namespace IdlePancake.Core.Abstractions
{
    public interface IEconomyFormulaService
    {
        long CalculateRecipeOutputPerSecond(string recipeId, int recipeLevel, long globalMultiplierPermille);
        long CalculateUpgradeCost(string upgradeId, int currentLevel);
        long CalculateRecipeUnlockCost(string recipeId, int unlockOrderIndex);
        long CalculatePrestigeReward(long totalCoinsEarnedLifetime, int currentPrestigeRank);
        long ApplyOfflineCap(long rawIncome, long maxAllowedIncome);
    }
}
