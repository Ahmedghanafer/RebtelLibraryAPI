using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.Commands.Books;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Commands.Books;

public class CreateBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly CreateBookCommandHandler _handler;
    private readonly Mock<ILogger<CreateBookCommandHandler>> _loggerMock;

    public CreateBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<CreateBookCommandHandler>>();
        _handler = new CreateBookCommandHandler(
            _bookRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateBookAndReturnBookDto()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Author",
            "1234567890123",
            250,
            "Fiction");

        var createdBook = Book.Create(
            command.Title,
            command.Author,
            command.ISBN,
            command.PageCount,
            command.Category);

        _bookRepositoryMock
            .Setup(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _bookRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Author, result.Author);
        Assert.Equal(command.ISBN, result.ISBN);
        Assert.Equal(command.PageCount, result.PageCount);
        Assert.Equal(command.Category, result.Category);
        Assert.Equal("Available", result.Availability);

        _bookRepositoryMock.Verify(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateISBN_ShouldThrowDomainException()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Author",
            "1234567890123",
            250,
            "Fiction");

        var existingBook = Book.Create(
            "Existing Book",
            "Existing Author",
            command.ISBN,
            200,
            "Non-Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookExistsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("already exists", exception.Message);
        Assert.Equal("BOOK_EXISTS", exception.ErrorCode);

        _bookRepositoryMock.Verify(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")] // Empty ISBN
    [InlineData("123456789")] // Too short
    [InlineData("12345678901234")] // Too long
    [InlineData("ABCDEFGHIJKLM")] // Non-numeric
    [InlineData(null)] // Null ISBN
    public async Task Handle_InvalidISBN_ShouldThrowDomainException(string isbn)
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Author",
            isbn!,
            250,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByISBNAsync(isbn!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("ISBN", exception.Message);
    }

    [Theory]
    [InlineData(-1)] // Negative page count
    [InlineData(0)] // Zero page count
    [InlineData(10001)] // Too many pages
    public async Task Handle_InvalidPageCount_ShouldThrowDomainException(int pageCount)
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Author",
            "1234567890123",
            pageCount,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

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
        var command = new CreateBookCommand(
            title!,
            "Test Author",
            "1234567890123",
            250,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

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
        var command = new CreateBookCommand(
            "Test Book",
            "Test Author",
            "1234567890123",
            250,
            category!);

        _bookRepositoryMock
            .Setup(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        if (string.IsNullOrEmpty(category))
            Assert.Contains("category", exception.Message);
        else
            Assert.Contains("Invalid category", exception.Message);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldThrowDomainException()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Author",
            "1234567890123",
            250,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByISBNAsync(command.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _bookRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Failed to create book", exception.Message);
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
    }
}