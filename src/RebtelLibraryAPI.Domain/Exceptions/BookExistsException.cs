namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when attempting to create a book that already exists
/// </summary>
public class BookExistsException : DomainException
{
    public BookExistsException(string message) : base(message, "BOOK_EXISTS")
    {
    }
}