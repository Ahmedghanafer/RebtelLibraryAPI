namespace RebtelLibraryAPI.Domain.Events;

public class BookCreatedEvent : DomainEvent
{
    public BookCreatedEvent(Guid bookId)
    {
        BookId = bookId;
    }

    public Guid BookId { get; }
}

public class BookUpdatedEvent : DomainEvent
{
    public BookUpdatedEvent(Guid bookId)
    {
        BookId = bookId;
    }

    public Guid BookId { get; }
}