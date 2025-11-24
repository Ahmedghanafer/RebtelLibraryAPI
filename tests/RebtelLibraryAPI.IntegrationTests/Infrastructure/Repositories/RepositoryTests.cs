using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Repositories;
using RebtelLibraryAPI.Infrastructure.Services;

namespace RebtelLibraryAPI.IntegrationTests.Infrastructure.Repositories;

/// <summary>
///     Focused integration tests for repository implementations
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly LibraryDbContext _context;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new LibraryDbContext(options);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task BookRepository_Should_Perform_Basic_CRUD_Operations()
    {
        // Arrange
        var logger = new Mock<ILogger<Repository<Book, Guid>>>().Object;
        var errorHandler = new Mock<DatabaseErrorHandler>(Mock.Of<ILogger<DatabaseErrorHandler>>()).Object;
        var repository = new BookRepository(_context, logger, errorHandler);

        var book = Book.Create("Test Book", "Test Author", "1234567890123", 200, "Fiction");

        // Act - Create
        var createdBook = await repository.AddAsync(book);
        createdBook.Should().NotBeNull();

        // Act - Read
        var retrievedBook = await repository.GetByIdAsync(book.Id);
        retrievedBook.Should().NotBeNull();
        retrievedBook!.Title.Should().Be("Test Book");

        // Act - Update
        var updatedBook = await repository.UpdateAsync(book);
        updatedBook.Should().NotBeNull();

        // Act - Delete
        await repository.DeleteAsync(book.Id);
        var deletedBook = await _context.Books.FindAsync(book.Id);
        deletedBook.Should().BeNull();
    }

    [Fact]
    public async Task BorrowerRepository_Should_Perform_Basic_CRUD_Operations()
    {
        // Arrange
        var logger = new Mock<ILogger<Repository<Borrower, Guid>>>().Object;
        var errorHandler = new Mock<DatabaseErrorHandler>(Mock.Of<ILogger<DatabaseErrorHandler>>()).Object;
        var repository = new BorrowerRepository(_context, logger, errorHandler);

        var borrower = Borrower.Create("John", "Doe", "john.doe@example.com", "555-555-1234");

        // Act - Create
        var createdBorrower = await repository.AddAsync(borrower);
        createdBorrower.Should().NotBeNull();

        // Act - Read
        var retrievedBorrower = await repository.GetByIdAsync(borrower.Id);
        retrievedBorrower.Should().NotBeNull();
        retrievedBorrower!.Email.Should().Be("john.doe@example.com");

        // Act - Update
        var updatedBorrower = await repository.UpdateAsync(borrower);
        updatedBorrower.Should().NotBeNull();

        // Act - Delete
        await repository.DeleteAsync(borrower.Id);
        var deletedBorrower = await _context.Borrowers.FindAsync(borrower.Id);
        deletedBorrower.Should().BeNull();
    }

    [Fact]
    public async Task Database_Connection_Should_Work()
    {
        // Act
        var canConnect = _context.Database.CanConnect();

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_Schema_Should_Be_Created()
    {
        // Act
        await _context.Database.EnsureCreatedAsync();

        // Assert - Just verify the model is created without errors
        _context.Model.Should().NotBeNull();

        // Verify all entity types are in the model
        _context.Model.FindEntityType(typeof(Book)).Should().NotBeNull();
        _context.Model.FindEntityType(typeof(Borrower)).Should().NotBeNull();
        _context.Model.FindEntityType(typeof(Loan)).Should().NotBeNull();
    }
}