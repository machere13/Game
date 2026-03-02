namespace IdlePancake.Core.Events
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
    }
}
