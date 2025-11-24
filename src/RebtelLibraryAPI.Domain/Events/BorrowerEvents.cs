namespace RebtelLibraryAPI.Domain.Events;

public class BorrowerRegisteredEvent : DomainEvent
{
    public BorrowerRegisteredEvent(Guid borrowerId)
    {
        BorrowerId = borrowerId;
    }

    public Guid BorrowerId { get; }
}

public class BorrowerUpdatedEvent : DomainEvent
{
    public BorrowerUpdatedEvent(Guid borrowerId)
    {
        BorrowerId = borrowerId;
    }

    public Guid BorrowerId { get; }
}