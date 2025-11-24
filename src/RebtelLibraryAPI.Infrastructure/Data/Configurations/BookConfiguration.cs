using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Infrastructure.Data.Configurations;

/// <summary>
///     Entity Framework Core configuration for the Book entity
/// </summary>
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        // Table configuration
        builder.ToTable("Books");

        // Configure EF Core to use parameterless constructor for materialization
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        // Primary key
        builder.HasKey(b => b.Id);

        // Property configurations
        builder.Property(b => b.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.ISBN)
            .IsRequired()
            .HasMaxLength(13)
            .IsUnicode(false); // ISBN is typically ASCII

        builder.Property(b => b.PageCount)
            .IsRequired();

        builder.Property(b => b.Category)
            .IsRequired()
            .HasMaxLength(50);

        // Base entity properties
        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(b => b.UpdatedAt)
            .ValueGeneratedOnUpdate();

        // Concurrency control - add shadow property for RowVersion
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken();

        // Indexes for performance
        builder.HasIndex(b => b.ISBN)
            .IsUnique()
            .HasDatabaseName("IX_Books_ISBN");

        builder.HasIndex(b => b.Title)
            .HasDatabaseName("IX_Books_Title");

        builder.HasIndex(b => b.Author)
            .HasDatabaseName("IX_Books_Author");

        builder.HasIndex(b => b.Category)
            .HasDatabaseName("IX_Books_Category");

        // Composite index for common query patterns
        builder.HasIndex(b => new { b.Category, b.Availability })
            .HasDatabaseName("IX_Books_Category_Availability");

        builder.HasIndex(b => new { b.Author, b.Title })
            .HasDatabaseName("IX_Books_Author_Title");

        // Handle value objects for enums
        builder.Property(b => b.Availability)
            .HasConversion<int>()
            .HasDefaultValue(BookAvailability.Available);
    }
}