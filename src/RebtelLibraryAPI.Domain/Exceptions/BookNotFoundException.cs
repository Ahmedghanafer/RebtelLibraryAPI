namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when a book is not found
/// </summary>
public class BookNotFoundException : DomainException
{
    public BookNotFoundException(string message) : base(message, "BOOK_NOT_FOUND")
    {
    }
}