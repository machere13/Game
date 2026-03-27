namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class Order
    {
        public RecipeConfig Recipe { get; }
        public int RewardCoins { get; }
        public int RewardXp { get; }

        public Order(RecipeConfig recipe)
        {
            Recipe = recipe;
            RewardCoins = recipe != null ? recipe.rewardCoins : 5;
            RewardXp = recipe != null ? recipe.rewardXp : 10;
        }
    }
}
