using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Interfaces;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Data.SeedData;
using RebtelLibraryAPI.Infrastructure.Repositories;
using RebtelLibraryAPI.Infrastructure.Services;

namespace RebtelLibraryAPI.Infrastructure;

/// <summary>
///     Dependency injection configuration for the Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds Infrastructure services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("RebtelLibraryAPI.Infrastructure");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        // Add database error handling service
        services.AddScoped<DatabaseErrorHandler>();

        // Add repositories
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBorrowerRepository, BorrowerRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();

        // Add database seeding service
        services.AddScoped<SqlSampleDataSeeder>();

        return services;
    }

    /// <summary>
    ///     Adds Infrastructure services with custom DbContext configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureDbContext">Action to configure DbContext</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<LibraryDbContext>(configureDbContext);

        // Add database error handling service
        services.AddScoped<DatabaseErrorHandler>();

        // Add repositories
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBorrowerRepository, BorrowerRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();

        // Add database seeding service
        services.AddScoped<SqlSampleDataSeeder>();

        return services;
    }
}