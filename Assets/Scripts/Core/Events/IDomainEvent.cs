using System;

namespace IdlePancake.Core.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredUtc { get; }
        string Name { get; }
    }
}
