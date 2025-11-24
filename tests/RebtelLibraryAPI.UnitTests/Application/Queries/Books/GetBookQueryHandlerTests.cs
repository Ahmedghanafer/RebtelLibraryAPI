using Microsoft.Extensions.Logging;
using Moq;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Application.Queries.Books;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Queries.Books;

public class GetBookQueryHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ILogger<GetBookQueryHandler>> _loggerMock;
    private readonly GetBookQueryHandler _handler;

    public GetBookQueryHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<GetBookQueryHandler>>();
        _handler = new GetBookQueryHandler(_bookRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidBookId_ShouldReturnBookDto()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = Book.Create("Test Book", "Test Author", "1234567890", 200, "Fiction");

        // Use reflection to set the ID to match our expected ID
        typeof(Entity<Guid>).GetProperty(nameof(Entity<Guid>.Id))!
            .SetValue(book, bookId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var query = new GetBookQuery(bookId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(bookId);
        result.Title.Should().Be("Test Book");
        result.Author.Should().Be("Test Author");
        result.ISBN.Should().Be("1234567890");
        result.PageCount.Should().Be(200);
        result.Category.Should().Be("Fiction");
        result.Availability.Should().Be("Available");

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentBookId_ShouldReturnNull()
    {
        // Arrange
        var bookId = Guid.NewGuid();

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var query = new GetBookQuery(bookId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyBookId_ShouldReturnNull()
    {
        // Arrange
        var query = new GetBookQuery(Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _bookRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var query = new GetBookQuery(bookId);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        // Assert
        exception.Should().Be(expectedException);
        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithBorrowedBook_ShouldReturnCorrectAvailability()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = Book.Create("Test Book", "Test Author", "1234567890123", 200, "Fiction");
        book.MarkAsBorrowed();

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var query = new GetBookQuery(bookId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Availability.Should().Be("Borrowed");
    }
}