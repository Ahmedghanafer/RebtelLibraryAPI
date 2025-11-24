namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when a borrower is not active
/// </summary>
public class BorrowerNotActiveException : DomainException
{
    public BorrowerNotActiveException(string message) : base(message, "BORROWER_NOT_ACTIVE")
    {
    }

    public BorrowerNotActiveException(string message, Exception innerException)
        : base(message, "BORROWER_NOT_ACTIVE", innerException)
    {
    }
}