namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when concurrency conflicts occur
/// </summary>
public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message) : base(message, "CONCURRENCY_ERROR")
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, "CONCURRENCY_ERROR", innerException)
    {
    }
}