using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RebtelLibraryAPI.Infrastructure.Data;

/// <summary>
///     Design-time factory for creating LibraryDbContext instances
///     Used by EF Core tools for migrations and scaffolding
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    /// <summary>
    ///     Creates a LibraryDbContext instance for design-time operations
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured LibraryDbContext</returns>
    public LibraryDbContext CreateDbContext(string[] args)
    {
        // Build configuration - look in API project directory
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "RebtelLibraryAPI.API");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        // Configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("RebtelLibraryAPI.Infrastructure");
        });

        // Enable sensitive data logging in development
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();

        return new LibraryDbContext(optionsBuilder.Options);
    }
}