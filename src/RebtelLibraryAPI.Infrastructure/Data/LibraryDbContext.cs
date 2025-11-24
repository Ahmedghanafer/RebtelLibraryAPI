using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Infrastructure.Data.Configurations;
using RebtelLibraryAPI.Infrastructure.Data.SeedData;

namespace RebtelLibraryAPI.Infrastructure.Data;

/// <summary>
///     Library database context for Entity Framework Core
/// </summary>
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    // DbSets for domain entities
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Borrower> Borrowers { get; set; } = null!;
    public DbSet<Loan> Loans { get; set; } = null!;

    /// <summary>
    /// Ensures the database is created and applies migrations
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
    {
        await Database.EnsureCreatedAsync(cancellationToken);
    }

    /// <summary>
    /// Applies pending migrations to the database
    /// </summary>
    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        var pendingMigrations = await Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            await Database.MigrateAsync(cancellationToken);
        }
    }

  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new BookConfiguration());
        modelBuilder.ApplyConfiguration(new BorrowerConfiguration());
        modelBuilder.ApplyConfiguration(new LoanConfiguration());

        // Set up query filters for soft deletes if needed
        modelBuilder.Entity<Book>().Property(b => b.Availability).HasConversion<int>();
        modelBuilder.Entity<Borrower>().Property(b => b.MemberStatus).HasConversion<int>();
        modelBuilder.Entity<Loan>().Property(l => l.Status).HasConversion<int>();
    }
}