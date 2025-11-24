using FluentAssertions;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.UnitTests.Domain;

public class BookTests
{
    [Fact]
    public void Create_ValidBook_ShouldCreateBook()
    {
        // Act
        var book = Book.Create("Test Title", "Test Author", "1234567890", 200, "Fiction");

        // Assert
        book.Should().NotBeNull();
        book.Title.Should().Be("Test Title");
        book.Author.Should().Be("Test Author");
        book.ISBN.Should().Be("1234567890");
        book.PageCount.Should().Be(200);
        book.Category.Should().Be("Fiction");
        book.Availability.Should().Be(BookAvailability.Available);
    }

    [Fact]
    public void Create_EmptyTitle_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<BookValidationException>(() =>
            Book.Create("", "Author", "1234567890", 200, "Fiction"));
    }

    [Fact]
    public void Create_EmptyAuthor_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<BookValidationException>(() =>
            Book.Create("Title", "", "1234567890", 200, "Fiction"));
    }

    [Fact]
    public void Create_InvalidPageCount_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<BookValidationException>(() =>
            Book.Create("Title", "Author", "1234567890", -1, "Fiction"));
    }

    [Fact]
    public void Create_InvalidISBN_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<BookValidationException>(() =>
            Book.Create("Title", "Author", "123", 200, "Fiction"));
    }

    [Fact]
    public void MarkAsBorrowed_ShouldUpdateAvailability()
    {
        // Arrange
        var book = Book.Create("Title", "Author", "1234567890", 200, "Fiction");

        // Act
        book.MarkAsBorrowed();

        // Assert
        book.Availability.Should().Be(BookAvailability.Borrowed);
    }

    [Fact]
    public void MarkAsBorrowed_AlreadyBorrowed_ShouldThrowException()
    {
        // Arrange
        var book = Book.Create("Title", "Author", "1234567890", 200, "Fiction");
        book.MarkAsBorrowed();

        // Act & Assert
        Assert.Throws<BookOperationException>(() => book.MarkAsBorrowed());
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateBookProperties()
    {
        // Arrange
        var book = Book.Create("Title", "Author", "1234567890", 200, "Fiction");

        // Act
        book.UpdateDetails("New Title", "New Author", 300, "Science");

        // Assert
        book.Title.Should().Be("New Title");
        book.Author.Should().Be("New Author");
        book.PageCount.Should().Be(300);
        book.Category.Should().Be("Science");
    }
}