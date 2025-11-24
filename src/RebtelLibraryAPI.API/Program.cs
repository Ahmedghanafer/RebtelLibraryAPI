using RebtelLibraryAPI.API.Middleware;
using RebtelLibraryAPI.API.Services;
using RebtelLibraryAPI.Application;
using RebtelLibraryAPI.Infrastructure;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Data.SeedData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GlobalExceptionInterceptor>();
});

// Add Application services (MediatR, CQRS handlers)
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Initialize database with migrations and sample data
await InitializeDatabaseAsync(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
app.MapGrpcService<LibraryGrpcService>();
app.MapGet("/", () => "Rebtel Library API gRPC Service - Use gRPC client to communicate with library services");

app.Run();

/// <summary>
/// Initializes the database with migrations and sample data
/// </summary>
static async Task InitializeDatabaseAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var sqlSeeder = scope.ServiceProvider.GetRequiredService<SqlSampleDataSeeder>();
    var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

    try
    {
        logger.LogInformation("Starting database initialization...");

        // Apply migrations
        await dbContext.ApplyMigrationsAsync();
        logger.LogInformation("Database migrations applied successfully");

        // Seed with sample data using SQL scripts (only if database is empty)
        await sqlSeeder.SeedAsync();
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization");
        throw;
    }
}

public partial class Program { } // Make the implicit Program class accessible to tests