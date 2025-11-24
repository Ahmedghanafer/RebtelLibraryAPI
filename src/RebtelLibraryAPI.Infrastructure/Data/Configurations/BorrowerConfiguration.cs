using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Infrastructure.Data.Configurations;

/// <summary>
///     Entity Framework Core configuration for the Borrower entity
/// </summary>
public class BorrowerConfiguration : IEntityTypeConfiguration<Borrower>
{
    public void Configure(EntityTypeBuilder<Borrower> builder)
    {
        // Table configuration
        builder.ToTable("Borrowers");

        // Primary key
        builder.HasKey(b => b.Id);

        // Property configurations
        builder.Property(b => b.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(b => b.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Email)
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false); // Email is typically ASCII

        builder.Property(b => b.Phone)
            .HasMaxLength(20);

        builder.Property(b => b.RegistrationDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

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
        builder.HasIndex(b => b.Email)
            .IsUnique()
            .HasDatabaseName("IX_Borrowers_Email");

        builder.HasIndex(b => b.LastName)
            .HasDatabaseName("IX_Borrowers_LastName");

        builder.HasIndex(b => b.FirstName)
            .HasDatabaseName("IX_Borrowers_FirstName");

        // Composite index for common query patterns
        builder.HasIndex(b => new { b.LastName, b.FirstName })
            .HasDatabaseName("IX_Borrowers_LastName_FirstName");

        builder.HasIndex(b => new { b.MemberStatus, b.RegistrationDate })
            .HasDatabaseName("IX_Borrowers_MemberStatus_RegistrationDate");

        // Handle value objects for enums
        builder.Property(b => b.MemberStatus)
            .HasConversion<int>()
            .HasDefaultValue(MemberStatus.Active);
    }
}