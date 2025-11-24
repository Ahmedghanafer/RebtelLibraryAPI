namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when borrower validation rules are violated
/// </summary>
public class BorrowerValidationException : DomainException
{
    public BorrowerValidationException(string message) : base(message, "BORROWER_VALIDATION_ERROR")
    {
    }

    public BorrowerValidationException(string message, Exception innerException)
        : base(message, "BORROWER_VALIDATION_ERROR", innerException)
    {
    }
}