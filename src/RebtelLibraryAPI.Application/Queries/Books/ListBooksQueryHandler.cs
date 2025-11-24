using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Books;

public class ListBooksQueryHandler : IRequestHandler<ListBooksQuery, ListBooksDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<ListBooksQueryHandler> _logger;

    public ListBooksQueryHandler(
        IBookRepository bookRepository,
        ILogger<ListBooksQueryHandler> logger)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListBooksDto> Handle(ListBooksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate pagination parameters
            if (request.PageNumber <= 0)
                request = request with { PageNumber = 1 };

            if (request.PageSize <= 0)
                request = request with { PageSize = 20 };
            else if (request.PageSize > 100)
                request = request with { PageSize = 100 };

            IEnumerable<Book> books;

            if (!string.IsNullOrWhiteSpace(request.CategoryFilter))
            {
                books = await _bookRepository.GetByCategoryAsync(request.CategoryFilter, cancellationToken);
                _logger.LogInformation("Retrieved books for category {Category}", request.CategoryFilter);
            }
            else
            {
                books = await _bookRepository.GetAllAsync(cancellationToken);
                _logger.LogInformation("Retrieved all books");
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLowerInvariant();
                books = books.Where(book =>
                    book.Title.ToLowerInvariant().Contains(searchTerm) ||
                    book.Author.ToLowerInvariant().Contains(searchTerm) ||
                    book.Category.ToLowerInvariant().Contains(searchTerm) ||
                    book.ISBN.Contains(searchTerm));

                _logger.LogInformation("Applied search filter for term: {SearchTerm}", request.SearchTerm);
            }

            var totalCount = books.Count();

            // Apply pagination
            var pagedBooks = books
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(MapToBookDto)
                .ToList();

            _logger.LogInformation(
                "Returning {BookCount} books, page {PageNumber} of {TotalPages}",
                pagedBooks.Count,
                request.PageNumber,
                (int)Math.Ceiling((double)totalCount / request.PageSize));

            return new ListBooksDto
            {
                Books = pagedBooks,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books list");
            throw;
        }
    }

    private static BookDto MapToBookDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PageCount = book.PageCount,
            Category = book.Category,
            Availability = book.Availability.ToString(),
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };
    }
}