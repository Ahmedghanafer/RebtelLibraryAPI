using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR;
using RebtelLibraryAPI.Application.Queries.Books;
using RebtelLibraryAPI.Application.Queries.Borrowers;

namespace RebtelLibraryAPI.Application;

/// <summary>
/// Dependency injection configuration for the Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}