using RebtelLibraryAPI.Domain.Events;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.Domain.Entities;

public class Book : Entity<Guid>
{
    private Book(
        Guid id,
        string title,
        string author,
        string isbn,
        int pageCount,
        string category,
        BookAvailability availability = BookAvailability.Available
    ) : base(id)
    {
        Title = title;
        Author = author;
        ISBN = isbn;
        PageCount = pageCount;
        Category = category;
        Availability = availability;
    }

    // Parameterless constructor for EF Core
    private Book()
    {
        Title = string.Empty;
        Author = string.Empty;
        ISBN = string.Empty;
        Category = string.Empty;
        Availability = BookAvailability.Available;
    }

    public string Title { get; private set; }
    public string Author { get; private set; }
    public string ISBN { get; private set; }
    public int PageCount { get; private set; }
    public string Category { get; private set; }
    public BookAvailability Availability { get; private set; }

    public static Book Create(
        string title,
        string author,
        string isbn,
        int pageCount,
        string category
    )
    {
        ValidateTitle(title);
        ValidateAuthor(author);
        ValidatePageCount(pageCount);
        ValidateCategory(category);
        ValidateISBN(isbn);

        var bookId = Guid.NewGuid();

        var book = new Book(bookId, title, author, isbn, pageCount, category);
        book.AddDomainEvent(new BookCreatedEvent(bookId));

        return book;
    }

    public void UpdateDetails(string title, string author, int pageCount, string category)
    {
        ValidateTitle(title);
        ValidateAuthor(author);
        ValidatePageCount(pageCount);
        ValidateCategory(category);

        Title = title;
        Author = author;
        PageCount = pageCount;
        Category = category;

        UpdateTimestamp();
        AddDomainEvent(new BookUpdatedEvent(Id));
    }

    public void UpdateAvailability(BookAvailability availability)
    {
        if (Availability == availability)
            return;

        Availability = availability;
        UpdateTimestamp();
    }

    public void MarkAsAvailable()
    {
        UpdateAvailability(BookAvailability.Available);
    }

    public void MarkAsBorrowed()
    {
        if (Availability != BookAvailability.Available)
            throw new BookOperationException("Only available books can be borrowed");

        UpdateAvailability(BookAvailability.Borrowed);
    }

    public void MarkAsReserved()
    {
        if (Availability != BookAvailability.Available)
            throw new BookOperationException("Only available books can be reserved");

        UpdateAvailability(BookAvailability.Reserved);
    }

    public void MarkUnderMaintenance()
    {
        UpdateAvailability(BookAvailability.Maintenance);
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BookValidationException("Book title is required");

        if (title.Length > 200)
            throw new BookValidationException("Book title cannot exceed 200 characters");
    }

    private static void ValidateAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new BookValidationException("Book author is required");

        if (author.Length > 100)
            throw new BookValidationException("Book author cannot exceed 100 characters");
    }

    private static void ValidatePageCount(int pageCount)
    {
        if (pageCount <= 0)
            throw new BookValidationException("Page count must be a positive number");

        if (pageCount > 10000)
            throw new BookValidationException("Page count cannot exceed 10,000 pages");
    }

    private static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new BookValidationException("Book category is required");

        if (category.Length > 50)
            throw new BookValidationException("Book category cannot exceed 50 characters");

        var validCategories = new[]
        {
            "Fiction", "Non-Fiction", "Science", "Technology", "Biography",
            "History", "Mystery", "Romance", "Children", "Reference",
            "Textbook", "Poetry", "Drama", "Fantasy", "Science Fiction"
        };

        if (!validCategories.Contains(category))
            throw new BookValidationException($"Invalid category: {category}");
    }

    private static void ValidateISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new BookValidationException("ISBN is required");

        // Simple validation: ISBN should be 10 or 13 characters (digits only, ignoring hyphens)
        var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");
        if (cleanIsbn.Length != 10 && cleanIsbn.Length != 13)
            throw new BookValidationException("ISBN must be 10 or 13 characters");

        // Validate that ISBN contains only digits (allowing X for ISBN-10 checksum)
        if (cleanIsbn.Length == 10)
        {
            if (!cleanIsbn.Substring(0, 9).All(char.IsDigit) ||
                !(cleanIsbn[9].Equals('X') || char.IsDigit(cleanIsbn[9])))
                throw new BookValidationException("ISBN must contain only digits (or X for ISBN-10)");
        }
        else if (cleanIsbn.Length == 13)
        {
            if (!cleanIsbn.All(char.IsDigit))
                throw new BookValidationException("ISBN must contain only digits");
        }
    }
}

public enum BookAvailability
{
    Available,
    Borrowed,
    Reserved,
    Maintenance
}