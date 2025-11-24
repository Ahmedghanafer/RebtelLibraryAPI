namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when book validation rules are violated
/// </summary>
public class BookValidationException : DomainException
{
    public BookValidationException(string message) : base(message, "BOOK_VALIDATION_ERROR")
    {
    }

    public BookValidationException(string message, Exception innerException)
        : base(message, "BOOK_VALIDATION_ERROR", innerException)
    {
    }
}