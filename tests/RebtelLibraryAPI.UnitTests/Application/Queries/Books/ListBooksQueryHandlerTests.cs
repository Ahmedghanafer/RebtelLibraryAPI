using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.Queries.Books;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Queries.Books;

public class ListBooksQueryHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly ListBooksQueryHandler _handler;
    private readonly Mock<ILogger<ListBooksQueryHandler>> _loggerMock;

    public ListBooksQueryHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<ListBooksQueryHandler>>();
        _handler = new ListBooksQueryHandler(_bookRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithDefaultParameters_ShouldReturnAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            Book.Create("Book 1", "Author 1", "1234567890123", 200, "Fiction"),
            Book.Create("Book 2", "Author 2", "1234567890124", 300, "Science")
        };

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var query = new ListBooksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Books.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(1);

        _bookRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnFilteredBooks()
    {
        // Arrange
        var fictionBooks = new List<Book>
        {
            Book.Create("Fiction Book 1", "Author 1", "1234567890123", 200, "Fiction"),
            Book.Create("Fiction Book 2", "Author 2", "1234567890124", 300, "Fiction")
        };

        _bookRepositoryMock
            .Setup(x => x.GetByCategoryAsync("Fiction", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fictionBooks);

        var query = new ListBooksQuery(CategoryFilter: "Fiction");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Books.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Books.All(b => b.Category == "Fiction").Should().BeTrue();

        _bookRepositoryMock.Verify(x => x.GetByCategoryAsync("Fiction", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var books = Enumerable.Range(1, 50)
            .Select(i => Book.Create($"Book {i}", $"Author {i}", "1234567890", 200, "Fiction"))
            .ToList();

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var query = new ListBooksQuery(2, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Books.Should().HaveCount(10);
        result.TotalCount.Should().Be(50);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(5);

        // Verify correct page is returned (books 11-20)
        result.Books.First().Title.Should().Be("Book 11");
        result.Books.Last().Title.Should().Be("Book 20");
    }

    [Fact]
    public async Task Handle_WithInvalidPageNumber_ShouldDefaultToPageOne()
    {
        // Arrange
        var books = new List<Book>
        {
            Book.Create("Book 1", "Author 1", "1234567890123", 200, "Fiction")
        };

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var query = new ListBooksQuery(-1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ShouldDefaultToTwenty()
    {
        // Arrange
        var books = new List<Book>
        {
            Book.Create("Book 1", "Author 1", "1234567890123", 200, "Fiction")
        };

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var query = new ListBooksQuery(PageSize: -1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_WithLargePageSize_ShouldLimitToOneHundred()
    {
        // Arrange
        var books = new List<Book>
        {
            Book.Create("Book 1", "Author 1", "1234567890123", 200, "Fiction")
        };

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var query = new ListBooksQuery(PageSize: 150);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(100); // Should limit to 100 when request exceeds maximum
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var query = new ListBooksQuery();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, CancellationToken.None));

        // Assert
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task Handle_WithEmptyBookList_ShouldReturnEmptyResult()
    {
        // Arrange
        var emptyBooks = new List<Book>();

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyBooks);

        var query = new ListBooksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Books.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Theory]
    [InlineData("Book", 10)] // Search term "Book" should match all 10 books with "Book" in title
    [InlineData("Author 1", 5)] // Search term should match specific author (5 Fiction books)
    [InlineData("Fiction", 5)] // Search term should match category (5 Fiction books)
    [InlineData("1234567890", 0)] // Search term should match ISBN pattern (none in test data)
    [InlineData("Nonexistent", 0)] // Search term should match nothing
    public async Task Handle_WithSearchTerm_ShouldReturnFilteredBooks(string searchTerm, int expectedCount)
    {
        // Arrange
        var books = CreateTestBooksWithCategories();
        var query = new ListBooksQuery(SearchTerm: searchTerm);

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(expectedCount);
        result.Books.Count.Should().Be(expectedCount);

        _bookRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCategoryAndSearch_ShouldApplyBothFilters()
    {
        // Arrange
        var books = CreateTestBooksWithCategories();
        var query = new ListBooksQuery(CategoryFilter: "Fiction", SearchTerm: "Author 1");

        _bookRepositoryMock
            .Setup(x => x.GetByCategoryAsync("Fiction", It.IsAny<CancellationToken>()))
            .ReturnsAsync(books.Where(b => b.Category == "Fiction").ToList().AsReadOnly());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Books.All(book => { return book.Category == "Fiction" && book.Author.Contains("Author 1"); }).Should()
            .BeTrue();

        _bookRepositoryMock.Verify(x => x.GetByCategoryAsync("Fiction", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CaseInsensitiveSearch_ShouldFindBooksRegardlessOfCase()
    {
        // Arrange
        var books = new List<Book>
        {
            Book.Create("The Great Gatsby", "F. Scott Fitzgerald", "9780743273565", 180, "Fiction"),
            Book.Create("TO KILL A MOCKINGBIRD", "Harper Lee", "9780061120084", 376, "Fiction")
        };
        var query = new ListBooksQuery(SearchTerm: "great");

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Books.Should().HaveCount(1);
        result.Books.First().Title.Should().Be("The Great Gatsby");
    }

    [Fact]
    public async Task Handle_EmptySearchTerm_ShouldReturnAllBooks()
    {
        // Arrange
        var books = CreateTestBooks(10);
        var query = new ListBooksQuery(SearchTerm: "   "); // Whitespace only

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(10);
        result.Books.Should().HaveCount(10);
    }

    private static List<Book> CreateTestBooks(int count)
    {
        var books = new List<Book>();
        for (var i = 1; i <= count; i++)
            books.Add(Book.Create(
                $"Book {i}",
                $"Author {i}",
                $"1234567890{i:D3}",
                200 + i,
                "Fiction"));
        return books;
    }

    private static List<Book> CreateTestBooksWithCategories()
    {
        var books = new List<Book>();
        for (var i = 1; i <= 5; i++)
            books.Add(Book.Create(
                $"Fiction Book {i}",
                "Author 1",
                $"1111111111{i:D3}",
                200 + i,
                "Fiction"));
        for (var i = 1; i <= 3; i++)
            books.Add(Book.Create(
                $"Science Book {i}",
                "Author 2",
                $"2222222222{i:D3}",
                300 + i,
                "Science"));
        for (var i = 1; i <= 2; i++)
            books.Add(Book.Create(
                $"History Book {i}",
                "Author 3",
                $"3333333333{i:D3}",
                400 + i,
                "History"));
        return books;
    }
}