namespace RebtelLibraryAPI.Domain.Events;

public class BookBorrowedEvent : DomainEvent
{
    public BookBorrowedEvent(Guid loanId, Guid bookId, Guid borrowerId)
    {
        LoanId = loanId;
        BookId = bookId;
        BorrowerId = borrowerId;
    }

    public Guid LoanId { get; }
    public Guid BookId { get; }
    public Guid BorrowerId { get; }
}

public class BookReturnedEvent : DomainEvent
{
    public BookReturnedEvent(Guid loanId, Guid bookId, Guid borrowerId)
    {
        LoanId = loanId;
        BookId = bookId;
        BorrowerId = borrowerId;
    }

    public Guid LoanId { get; }
    public Guid BookId { get; }
    public Guid BorrowerId { get; }
}