namespace RebtelLibraryAPI.Domain.Exceptions;

/// <summary>
///     Exception thrown when database access errors occur
/// </summary>
public class DataAccessException : DomainException
{
    public DataAccessException(string message) : base(message, "DATA_ACCESS_ERROR")
    {
    }

    public DataAccessException(string message, Exception innerException)
        : base(message, "DATA_ACCESS_ERROR", innerException)
    {
    }
}