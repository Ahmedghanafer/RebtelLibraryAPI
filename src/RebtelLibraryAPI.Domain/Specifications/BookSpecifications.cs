using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Domain.Specifications;

public class BookMustBeAvailable : Specification<Book>
{
    public override string ErrorMessage => "Book must be available for borrowing";

    public override bool IsSatisfiedBy(Book book)
    {
        return book.Availability == BookAvailability.Available;
    }
}

public class BookMustHaveValidPageCount : Specification<Book>
{
    public override string ErrorMessage => "Book must have valid page count (1-10000 pages)";

    public override bool IsSatisfiedBy(Book book)
    {
        return book.PageCount > 0 && book.PageCount <= 10000;
    }
}

public class BookMustHaveValidCategory : Specification<Book>
{
    private static readonly string[] ValidCategories = new[]
    {
        "Fiction", "Non-Fiction", "Science", "Technology", "Biography",
        "History", "Mystery", "Romance", "Children", "Reference",
        "Textbook", "Poetry", "Drama", "Fantasy", "Science Fiction"
    };

    public override string ErrorMessage => "Book must have a valid category";

    public override bool IsSatisfiedBy(Book book)
    {
        return !string.IsNullOrWhiteSpace(book.Category) &&
               ValidCategories.Contains(book.Category);
    }
}

public class BookMustHaveValidTitle : Specification<Book>
{
    public override string ErrorMessage => "Book must have a title (max 200 characters)";

    public override bool IsSatisfiedBy(Book book)
    {
        return !string.IsNullOrWhiteSpace(book.Title) &&
               book.Title.Length <= 200;
    }
}

public class BookMustHaveValidAuthor : Specification<Book>
{
    public override string ErrorMessage => "Book must have an author (max 100 characters)";

    public override bool IsSatisfiedBy(Book book)
    {
        return !string.IsNullOrWhiteSpace(book.Author) &&
               book.Author.Length <= 100;
    }
}