using System;

namespace IdlePancake.Core.Abstractions
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
        TimeSpan Elapsed(DateTime fromUtc, DateTime toUtc);
    }
}
