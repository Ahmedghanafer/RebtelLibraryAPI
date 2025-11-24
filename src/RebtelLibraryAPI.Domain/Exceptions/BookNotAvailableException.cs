namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when a book is not available for borrowing
/// </summary>
public class BookNotAvailableException : DomainException
{
    public BookNotAvailableException(string message) : base(message, "BOOK_NOT_AVAILABLE")
    {
    }

    public BookNotAvailableException(string message, Exception innerException)
        : base(message, "BOOK_NOT_AVAILABLE", innerException)
    {
    }
}