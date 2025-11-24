namespace RebtelLibraryAPI.Domain.Events;

public interface IDomainEventProvider
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}