namespace IdlePancake.Features.Upgrades.Application
{
    public interface IBuyUpgradeUseCase
    {
        bool Execute(string upgradeId);
    }
}
