using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Infrastructure.Data.Configurations;

/// <summary>
///     Entity Framework Core configuration for the Loan entity
/// </summary>
public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        // Table configuration
        builder.ToTable("Loans");

        // Primary key
        builder.HasKey(l => l.Id);

        // Property configurations
        builder.Property(l => l.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(l => l.BookId)
            .IsRequired();

        builder.Property(l => l.BorrowerId)
            .IsRequired();

        builder.Property(l => l.BorrowDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(l => l.DueDate)
            .IsRequired();

        builder.Property(l => l.ReturnDate)
            .IsRequired(false);

        // Base entity properties
        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(l => l.UpdatedAt)
            .ValueGeneratedOnUpdate();

        // Concurrency control - add shadow property for RowVersion
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken();

        // Foreign key relationships
        builder.HasOne<Book>()
            .WithMany()
            .HasForeignKey(l => l.BookId)
            .HasConstraintName("FK_Loans_Books_BookId")
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting books with active loans

        builder.HasOne<Borrower>()
            .WithMany()
            .HasForeignKey(l => l.BorrowerId)
            .HasConstraintName("FK_Loans_Borrowers_BorrowerId")
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting borrowers with active loans

        // Indexes for performance
        builder.HasIndex(l => l.BookId)
            .HasDatabaseName("IX_Loans_BookId");

        builder.HasIndex(l => l.BorrowerId)
            .HasDatabaseName("IX_Loans_BorrowerId");

        builder.HasIndex(l => l.BorrowDate)
            .HasDatabaseName("IX_Loans_BorrowDate");

        builder.HasIndex(l => l.DueDate)
            .HasDatabaseName("IX_Loans_DueDate");

        builder.HasIndex(l => l.ReturnDate)
            .HasDatabaseName("IX_Loans_ReturnDate");

        // Composite indexes for common query patterns
        builder.HasIndex(l => new { l.BookId, l.Status })
            .HasDatabaseName("IX_Loans_BookId_Status");

        builder.HasIndex(l => new { l.BorrowerId, l.Status })
            .HasDatabaseName("IX_Loans_BorrowerId_Status");

        builder.HasIndex(l => new { l.Status, l.DueDate })
            .HasDatabaseName("IX_Loans_Status_DueDate");

        builder.HasIndex(l => new { l.BorrowDate, l.BookId })
            .HasDatabaseName("IX_Loans_BorrowDate_BookId");

        // Unique constraint to prevent duplicate active loans for the same book
        builder.HasIndex(l => new { l.BookId, l.Status })
            .HasDatabaseName("UX_Loans_BookId_ActiveLoan")
            .IsUnique()
            .HasFilter("[Status] = 0"); // Only for active loans (Status = 0)

        // Handle value objects for enums
        builder.Property(l => l.Status)
            .HasConversion<int>()
            .HasDefaultValue(LoanStatus.Active);
    }
}