namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class Order
    {
        public RecipeConfig Recipe { get; }
        public int RewardCoins { get; }
        public int RewardXp { get; }
        public int PersonIndex { get; }

        public Order(RecipeConfig recipe, int personIndex = 0)
        {
            Recipe = recipe;
            RewardCoins = recipe != null ? recipe.rewardCoins : 5;
            RewardXp = recipe != null ? recipe.rewardXp : 10;
            PersonIndex = personIndex;
        }
    }
}
