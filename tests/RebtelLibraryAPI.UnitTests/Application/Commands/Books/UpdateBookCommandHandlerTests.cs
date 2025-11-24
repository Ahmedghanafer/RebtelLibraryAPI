using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.Commands.Books;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Commands.Books;

public class UpdateBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly UpdateBookCommandHandler _handler;
    private readonly Mock<ILogger<UpdateBookCommandHandler>> _loggerMock;

    public UpdateBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<UpdateBookCommandHandler>>();
        _handler = new UpdateBookCommandHandler(
            _bookRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateBookAndReturnBookDto()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand(
            bookId,
            "Updated Book Title",
            "Updated Author",
            300,
            "Science",
            true);

        var existingBook = Book.Create(
            "Original Title",
            "Original Author",
            "1234567890123",
            200,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _bookRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Author, result.Author);
        Assert.Equal("1234567890123", result.ISBN); // ISBN should remain unchanged
        Assert.Equal(command.PageCount, result.PageCount);
        Assert.Equal(command.Category, result.Category);
        Assert.Equal("Available", result.Availability);

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BookNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand(
            bookId,
            "Updated Book Title",
            "Updated Author",
            300,
            "Science",
            true);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("not found", exception.Message);
        Assert.Equal("BOOK_NOT_FOUND", exception.ErrorCode);

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)] // Negative page count
    [InlineData(0)] // Zero page count
    [InlineData(10001)] // Too many pages
    public async Task Handle_InvalidPageCount_ShouldThrowDomainException(int pageCount)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand(
            bookId,
            "Updated Book Title",
            "Updated Author",
            pageCount,
            "Science",
            true);

        var existingBook = Book.Create(
            "Original Title",
            "Original Author",
            "1234567890123",
            200,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Page count", exception.Message);
    }

    [Theory]
    [InlineData("")] // Empty title
    [InlineData(null)] // Null title
    public async Task Handle_InvalidTitle_ShouldThrowDomainException(string title)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand(
            bookId,
            title!,
            "Updated Author",
            300,
            "Science",
            true);

        var existingBook = Book.Create(
            "Original Title",
            "Original Author",
            "1234567890123",
            200,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("title", exception.Message);
    }

    [Theory]
    [InlineData("InvalidCategory")] // Invalid category
    [InlineData("")] // Empty category
    [InlineData(null)] // Null category
    public async Task Handle_InvalidCategory_ShouldThrowDomainException(string category)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand(
            bookId,
            "Updated Book Title",
            "Updated Author",
            300,
            category!,
            true);

        var existingBook = Book.Create(
            "Original Title",
            "Original Author",
            "1234567890123",
            200,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        if (string.IsNullOrEmpty(category))
            Assert.Contains("category", exception.Message);
        else
            Assert.Contains("Invalid category", exception.Message);
    }

    [Fact]
    public async Task Handle_SetBookAsUnavailable_ShouldUpdateBookAvailability()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand(
            bookId,
            "Updated Book Title",
            "Updated Author",
            300,
            "Science",
            false); // Set as unavailable

        var existingBook = Book.Create(
            "Original Title",
            "Original Author",
            "1234567890123",
            200,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _bookRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Borrowed", result.Availability); // Should be set to Borrowed when IsAvailable is false
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldThrowDomainException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new UpdateBookCommand(
            bookId,
            "Updated Book Title",
            "Updated Author",
            300,
            "Science",
            true);

        var existingBook = Book.Create(
            "Original Title",
            "Original Author",
            "1234567890123",
            200,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _bookRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Failed to update book", exception.Message);
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
    }
}