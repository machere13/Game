using IdlePancake.Core.Models;

namespace IdlePancake.Features.SaveLoad.Application
{
    public interface ISaveMigrationPipeline
    {
        SaveState UpgradeToLatest(SaveState state);
    }
}
