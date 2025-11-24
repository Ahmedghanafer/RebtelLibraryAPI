using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs.Analytics;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Analytics;

public class GetBookRecommendationsQueryHandler : IRequestHandler<GetBookRecommendationsQuery, BooksAnalyticsResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<GetBookRecommendationsQueryHandler> _logger;

    public GetBookRecommendationsQueryHandler(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        ILogger<GetBookRecommendationsQueryHandler> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BooksAnalyticsResponse> Handle(GetBookRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting book recommendations for book {BookId} with limit {Limit}",
            request.BookId, request.Limit);

        try
        {
            // Validate input parameters
            ValidateInputParameters(request);

            // Get borrowers who borrowed the specified book
            var targetBookBorrowers = await GetBorrowersWhoBorrowedBookAsync(request.BookId, cancellationToken);

            if (!targetBookBorrowers.Any())
            {
                _logger.LogInformation("No borrowers found for book {BookId}", request.BookId);
                return CreateEmptyResponse();
            }

            // Get books borrowed by these borrowers (excluding the target book)
            var recommendedBooks =
                await GetRecommendedBooksAsync(targetBookBorrowers, request.BookId, cancellationToken);

            if (!recommendedBooks.Any())
            {
                _logger.LogInformation("No recommended books found for book {BookId}", request.BookId);
                return CreateEmptyResponse();
            }

            // Apply limit and order by recommendation score
            var limitedRecommendations = recommendedBooks
                .OrderByDescending(b => b.BorrowCount)
                .ThenBy(b => b.Title)
                .Take(request.Limit)
                .ToList();

            var response = new BooksAnalyticsResponse
            {
                Books = limitedRecommendations,
                TotalCount = limitedRecommendations.Count,
                Page = 1,
                PageSize = limitedRecommendations.Count,
                HasNextPage = false
            };

            _logger.LogInformation("Retrieved {BookCount} book recommendations for book {BookId}",
                response.Books.Count, request.BookId);

            return response;
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book recommendations for book {BookId}", request.BookId);
            throw new ValidationException("Failed to get book recommendations", ex);
        }
    }

    private static void ValidateInputParameters(GetBookRecommendationsQuery request)
    {
        if (request.BookId == Guid.Empty)
            throw new ValidationException("Book ID cannot be empty");

        if (request.Limit <= 0 || request.Limit > 50)
            throw new ValidationException("Limit must be between 1 and 50");
    }

    private async Task<List<Guid>> GetBorrowersWhoBorrowedBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var borrowers = await _loanRepository.GetBorrowersWhoBorrowedBookAsync(bookId, cancellationToken);
        return borrowers.ToList();
    }

    private async Task<List<BookAnalyticsDto>> GetRecommendedBooksAsync(
        List<Guid> borrowerIds,
        Guid excludeBookId,
        CancellationToken cancellationToken)
    {
        // Get books borrowed by these borrowers
        var recommendedBooks = await _bookRepository.GetBooksBorrowedByUsersAsync(borrowerIds, cancellationToken);

        // Filter out the target book and convert to analytics DTOs
        var bookAnalytics = recommendedBooks
            .Where(b => b.Id != excludeBookId)
            .Select(book => new BookAnalyticsDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                BorrowCount = 0, // We'll calculate this separately if needed
                PageCount = book.PageCount,
                Category = book.Category
            })
            .ToList();

        // For a more accurate recommendation, we could count actual borrow occurrences,
        // but for now we'll return all books borrowed by similar users
        return bookAnalytics;
    }

    private static BooksAnalyticsResponse CreateEmptyResponse()
    {
        return new BooksAnalyticsResponse
        {
            Books = new List<BookAnalyticsDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 0,
            HasNextPage = false
        };
    }
}