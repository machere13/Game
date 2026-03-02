namespace IdlePancake.Features.Recipes.Application
{
    public interface IRecipeUnlockUseCase
    {
        bool Execute(string recipeId);
    }
}
