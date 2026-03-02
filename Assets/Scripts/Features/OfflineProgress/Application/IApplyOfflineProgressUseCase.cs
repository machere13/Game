using System;

namespace IdlePancake.Features.OfflineProgress.Application
{
    public interface IApplyOfflineProgressUseCase
    {
        void Execute(DateTime nowUtc);
    }
}
