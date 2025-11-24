using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Books;

public class GetBookQueryHandler : IRequestHandler<GetBookQuery, BookDto?>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBookQueryHandler> _logger;

    public GetBookQueryHandler(
        IBookRepository bookRepository,
        ILogger<GetBookQueryHandler> logger)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookDto?> Handle(GetBookQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            _logger.LogWarning("GetBookQuery called with empty Id");
            return null;
        }

        try
        {
            var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);

            if (book == null)
            {
                _logger.LogInformation("Book with Id {BookId} not found", request.Id);
                return null;
            }

            _logger.LogInformation("Retrieved book with Id {BookId}", request.Id);

            return MapToBookDto(book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving book with Id {BookId}", request.Id);
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