namespace RebtelLibraryAPI.Domain.Exceptions;

public class BookOperationException : DomainException
{
    public BookOperationException(string message) : base(message, "BOOK_OPERATION_ERROR")
    {
    }

    public BookOperationException(string message, Exception innerException)
        : base(message, "BOOK_OPERATION_ERROR", innerException)
    {
    }
}