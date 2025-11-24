namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when a borrower is not found
/// </summary>
public class BorrowerNotFoundException : DomainException
{
    public BorrowerNotFoundException(string message) : base(message, "BORROWER_NOT_FOUND")
    {
    }
}