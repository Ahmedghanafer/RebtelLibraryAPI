using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.Infrastructure.Services;

/// <summary>
///     Service for handling database errors and mapping them to domain exceptions
/// </summary>
public class DatabaseErrorHandler
{
    private readonly ILogger<DatabaseErrorHandler> _logger;

    public DatabaseErrorHandler(ILogger<DatabaseErrorHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Handles database exceptions and converts them to appropriate domain exceptions
    /// </summary>
    /// <param name="exception">The original database exception</param>
    /// <param name="entityName">Name of the entity being operated on</param>
    /// <param name="operation">The operation being performed</param>
    /// <exception cref="DomainException">Mapped domain exception</exception>
    public void HandleDatabaseException(Exception exception, string entityName, string operation)
    {
        _logger.LogError(exception, "Database operation failed for {EntityName} during {Operation}: {ErrorMessage}",
            entityName, operation, exception.Message);

        switch (exception)
        {
            case DbUpdateConcurrencyException concurrencyEx:
                _logger.LogWarning("Concurrency conflict detected for {EntityName}: {Details}",
                    entityName, concurrencyEx.Message);
                throw new ConcurrencyException($"The {entityName} was modified by another user. Please refresh and try again.");

            case DbUpdateException updateEx when updateEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx:
                HandleSqlException(sqlEx, entityName, operation);
                break;

            case DbUpdateException updateEx:
                _logger.LogError(updateEx, "Database update failed for {EntityName}: {Details}",
                    entityName, updateEx.InnerException?.Message ?? updateEx.Message);
                throw new DataAccessException($"Failed to {operation.ToLower()} {entityName}. Please try again later.");

            case InvalidOperationException operationEx when operationEx.Message.Contains("Connection string"):
                _logger.LogError(operationEx, "Database configuration error: {Details}", operationEx.Message);
                throw new DataAccessException("Database configuration error. Please contact system administrator.");

            case TimeoutException timeoutEx:
                _logger.LogError(timeoutEx, "Database operation timeout for {EntityName}: {Details}",
                    entityName, timeoutEx.Message);
                throw new DataAccessException($"Database operation timed out. Please try again.");

            default:
                _logger.LogError(exception, "Unexpected database error for {EntityName}: {Details}",
                    entityName, exception.Message);
                throw new DataAccessException($"An unexpected error occurred while accessing the database for {entityName}.");
        }
    }

    /// <summary>
    ///     Handles specific SQL Server exceptions
    /// </summary>
    private void HandleSqlException(Microsoft.Data.SqlClient.SqlException sqlEx, string entityName, string operation)
    {
        switch (sqlEx.Number)
        {
            case 2601: // Unique constraint violation
            case 2627: // Unique index constraint violation
                _logger.LogWarning("Unique constraint violation for {EntityName}: {Details}", entityName, sqlEx.Message);
                if (sqlEx.Message.Contains("ISBN"))
                {
                    throw new BookValidationException("A book with this ISBN already exists.");
                }
                if (sqlEx.Message.Contains("Email"))
                {
                    throw new BorrowerValidationException("A borrower with this email already exists.");
                }
                throw new ValidationException($"A {entityName.ToLower()} with these details already exists.");

            case 547: // Foreign key constraint violation
                _logger.LogWarning("Foreign key constraint violation for {EntityName}: {Details}", entityName, sqlEx.Message);
                if (sqlEx.Message.Contains("FK_Loans_Books_BookId"))
                {
                    throw new BookValidationException("Cannot delete book with active loans.");
                }
                if (sqlEx.Message.Contains("FK_Loans_Borrowers_BorrowerId"))
                {
                    throw new BorrowerValidationException("Cannot delete borrower with active loans.");
                }
                throw new ValidationException($"Referential integrity violation for {entityName}.");

            case 515: // Cannot insert the value NULL into column
                _logger.LogWarning("NULL value violation for {EntityName}: {Details}", entityName, sqlEx.Message);
                throw new ValidationException($"Required field is missing for {entityName}.");

            case 208: // Invalid object name (table doesn't exist)
                _logger.LogError(sqlEx, "Database object not found for {EntityName}: {Details}", entityName, sqlEx.Message);
                throw new DataAccessException("Database schema error. Please contact system administrator.");

            case 4060: // Cannot open database requested by login
                _logger.LogError(sqlEx, "Database access error: {Details}", sqlEx.Message);
                throw new DataAccessException("Database access denied. Please contact system administrator.");

            case 18456: // Login failed for user
                _logger.LogError(sqlEx, "Database authentication error: {Details}", sqlEx.Message);
                throw new DataAccessException("Database authentication failed. Please contact system administrator.");

            default:
                _logger.LogError(sqlEx, "SQL Server error for {EntityName} (Error {ErrorNumber}): {Details}",
                    entityName, sqlEx.Number, sqlEx.Message);
                throw new DataAccessException($"Database error occurred while {operation.ToLower()} {entityName}.");
        }
    }

    /// <summary>
    ///     Logs successful database operations for audit purposes
    /// </summary>
    /// <param name="entityName">Name of the entity</param>
    /// <param name="operation">Operation performed</param>
    /// <param name="entityId">ID of the entity (if available)</param>
    public void LogSuccessfulOperation(string entityName, string operation, Guid? entityId = null)
    {
        _logger.LogInformation("Successfully {Operation} {EntityName} {EntityId}",
            operation, entityName, entityId.HasValue ? $"with ID {entityId.Value}" : "");
    }

    /// <summary>
    ///     Logs database operation warnings
    /// </summary>
    /// <param name="message">Warning message</param>
    /// <param name="entityName">Entity name (optional)</param>
    /// <param name="operation">Operation (optional)</param>
    public void LogWarning(string message, string? entityName = null, string? operation = null)
    {
        if (!string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(operation))
        {
            _logger.LogWarning("Database warning during {Operation} on {EntityName}: {Message}",
                operation, entityName, message);
        }
        else
        {
            _logger.LogWarning("Database warning: {Message}", message);
        }
    }
}