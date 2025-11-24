namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when validation rules are violated
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message, "VALIDATION_ERROR")
    {
    }

    public ValidationException(string message, Exception innerException)
        : base(message, "VALIDATION_ERROR", innerException)
    {
    }
}