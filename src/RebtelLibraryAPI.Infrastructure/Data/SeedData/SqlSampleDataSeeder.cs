using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Infrastructure.Data;

namespace RebtelLibraryAPI.Infrastructure.Data.SeedData;

/// <summary>
/// SQL-based sample data seeder for the library management system
/// </summary>
public class SqlSampleDataSeeder
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<SqlSampleDataSeeder> _logger;
    private readonly string _seedScriptsPath;

    public SqlSampleDataSeeder(
        LibraryDbContext context,
        ILogger<SqlSampleDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
        // Use relative path that works when running from API project
        _seedScriptsPath = Path.Combine("Data", "SeedData", "SqlSeedScripts");
    }

    /// <summary>
    /// Seeds the database with sample data using SQL scripts
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if database already has data
            var hasExistingData = await HasExistingDataAsync(cancellationToken);
            if (hasExistingData)
            {
                _logger.LogInformation("Database already contains data. Skipping SQL seeding.");
                return;
            }

            _logger.LogInformation("Database is empty. Starting SQL sample data seeding...");
        _logger.LogInformation("Using seed scripts path: {SeedScriptsPath}", _seedScriptsPath);

            // Execute SQL scripts in order
            await ExecuteSqlScriptAsync("01_Books.sql", cancellationToken);
            await ExecuteSqlScriptAsync("02_Borrowers.sql", cancellationToken);
            await ExecuteSqlScriptAsync("03_Loans.sql", cancellationToken);

            _logger.LogInformation("SQL sample data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during SQL sample data seeding");
            throw;
        }
    }

    /// <summary>
    /// Checks if the database already contains data
    /// </summary>
    private async Task<bool> HasExistingDataAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books.AnyAsync(cancellationToken) ||
               await _context.Borrowers.AnyAsync(cancellationToken) ||
               await _context.Loans.AnyAsync(cancellationToken);
    }

    
    /// <summary>
    /// Executes a SQL script file
    /// </summary>
    private async Task ExecuteSqlScriptAsync(string scriptName, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptPath = Path.Combine(_seedScriptsPath, scriptName);

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"SQL seed script not found: {scriptPath}");
            }

            var sqlContent = await File.ReadAllTextAsync(scriptPath, cancellationToken);

            if (string.IsNullOrWhiteSpace(sqlContent))
            {
                _logger.LogWarning("SQL script {ScriptName} is empty", scriptName);
                return;
            }

            _logger.LogInformation("Executing SQL script: {ScriptName}", scriptName);

            // Execute the SQL script
            var affectedRows = await _context.Database.ExecuteSqlRawAsync(sqlContent, cancellationToken);

            _logger.LogInformation("SQL script {ScriptName} executed successfully. Affected rows: {AffectedRows}",
                scriptName, affectedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL script: {ScriptName}", scriptName);
            throw;
        }
    }
}