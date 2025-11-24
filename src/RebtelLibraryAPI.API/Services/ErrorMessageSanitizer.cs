using System.Text.RegularExpressions;

namespace RebtelLibraryAPI.API.Services;

/// <summary>
///     Provides methods for sanitizing error messages to prevent information disclosure
/// </summary>
public static class ErrorMessageSanitizer
{
    private const string DefaultErrorMessage = "An error occurred while processing your request";

    /// <summary>
    ///     Sanitizes exception messages for client exposure
    /// </summary>
    /// <param name="exception">The exception to sanitize</param>
    /// <returns>A sanitized error message safe for client exposure</returns>
    public static string Sanitize(Exception exception)
    {
        if (exception == null)
            return DefaultErrorMessage;

        var message = exception.Message;

        // Check if it's a known business exception that we want to expose
        if (IsBusinessException(exception)) return SanitizeBusinessException(message);

        // For all other exceptions, return a generic message
        return DefaultErrorMessage;
    }

    /// <summary>
    ///     Checks if an exception is a known business exception that can be safely exposed
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if it's a safe business exception, false otherwise</returns>
    private static bool IsBusinessException(Exception exception)
    {
        var exceptionTypeName = exception.GetType().Name;

        return exceptionTypeName.Contains("ValidationException") ||
               exceptionTypeName.Contains("NotFoundException") ||
               exceptionTypeName.Contains("DuplicateException") ||
               exceptionTypeName.Contains("BusinessRuleException");
    }

    /// <summary>
    ///     Sanitizes business exception messages while preserving business logic
    /// </summary>
    /// <param name="message">The business exception message</param>
    /// <returns>A sanitized business exception message</returns>
    private static string SanitizeBusinessException(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return DefaultErrorMessage;

        // Remove any potentially sensitive information from business exception messages
        // Common patterns to avoid exposing:
        // - Stack traces
        // - Internal system details
        // - Database connection strings
        // - File paths
        // - Server configuration details

        // Limit message length to prevent message overflow attacks
        const int maxMessageLength = 200;
        if (message.Length > maxMessageLength) message = message.Substring(0, maxMessageLength) + "...";

        // Remove any potential stack trace patterns
        var sanitized = Regex.Replace(message,
            @"at\s+[\w\d\.]+\([^)]*\)\s+in\s+[^:]+:\d+", "",
            RegexOptions.IgnoreCase);

        // Remove file paths
        sanitized = Regex.Replace(sanitized,
            @"[a-zA-Z]:\\[^:]*|/[^:]*", "path",
            RegexOptions.IgnoreCase);

        // Remove SQL error patterns
        sanitized = Regex.Replace(sanitized,
            @"SQL\s+Server|Connection\s+String|Database\s+Error", "",
            RegexOptions.IgnoreCase);

        // Clean up any whitespace artifacts from replacements
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();

        return string.IsNullOrWhiteSpace(sanitized) ? DefaultErrorMessage : sanitized;
    }
}