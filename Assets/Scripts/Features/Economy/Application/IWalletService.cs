namespace IdlePancake.Features.Economy.Application
{
    public interface IWalletService
    {
        long GetCoins();
        long GetPancakeTokens();
        bool TrySpendCoins(long amount);
        void AddCoins(long amount);
        void AddPancakeTokens(long amount);
    }
}
