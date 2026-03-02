using IdlePancake.Core.Models;

namespace IdlePancake.Core.Abstractions
{
    public interface ISaveRepository
    {
        bool Exists();
        SaveState Load();
        void Save(SaveState snapshot);
    }
}
