using Microsoft.Extensions.DependencyInjection;
using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Domain.Events;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IEnumerable<Entity<Guid>> entities, CancellationToken cancellationToken = default);
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAndClearEvents(IEnumerable<Entity<Guid>> entities,
        CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
            if (entity is IDomainEventProvider eventProvider)
            {
                var domainEvents = eventProvider.DomainEvents.ToList();

                foreach (var domainEvent in domainEvents) await DispatchDomainEvent(domainEvent, cancellationToken);

                entity.GetType()
                    .GetMethod(nameof(entity.ClearDomainEvents))
                    ?.Invoke(entity, null);
            }
    }

    private async Task DispatchDomainEvent(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
            if (handler is IDomainEventHandler<IDomainEvent> typedHandler)
                await typedHandler.Handle(domainEvent, cancellationToken);
    }
}