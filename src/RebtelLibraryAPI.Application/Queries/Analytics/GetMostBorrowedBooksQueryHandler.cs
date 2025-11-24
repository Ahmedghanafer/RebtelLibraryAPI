using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs.Analytics;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Analytics;

public class GetMostBorrowedBooksQueryHandler : IRequestHandler<GetMostBorrowedBooksQuery, BooksAnalyticsResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<GetMostBorrowedBooksQueryHandler> _logger;

    public GetMostBorrowedBooksQueryHandler(
        ILoanRepository loanRepository,
        ILogger<GetMostBorrowedBooksQueryHandler> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BooksAnalyticsResponse> Handle(GetMostBorrowedBooksQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting most borrowed books from {StartDate} to {EndDate}, page {Page}, size {PageSize}",
            request.StartDate, request.EndDate, request.Page, request.PageSize);

        try
        {
            // Validate input parameters
            ValidateInputParameters(request);

            // Get most borrowed books analytics directly from repository
            var mostBorrowedBooks = await _loanRepository.GetMostBorrowedBooksAsync(
                request.StartDate, request.EndDate, cancellationToken);

            if (!mostBorrowedBooks.Any())
            {
                _logger.LogInformation("No completed loans found in the specified date range");
                return CreateEmptyResponse(request);
            }

            // Convert domain DTOs to application DTOs
            var bookAnalytics = mostBorrowedBooks
                .Select(b => new BookAnalyticsDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    BorrowCount = b.BorrowCount,
                    PageCount = b.PageCount,
                    Category = b.Category
                })
                .ToList();

            // Apply pagination
            var totalCount = bookAnalytics.Count;
            var pagedBooks = bookAnalytics
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var response = new BooksAnalyticsResponse
            {
                Books = pagedBooks,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                HasNextPage = request.Page * request.PageSize < totalCount
            };

            _logger.LogInformation("Retrieved {BookCount} most borrowed books (total: {TotalCount})",
                response.Books.Count, totalCount);

            return response;
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving most borrowed books from {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);
            throw new ValidationException("Failed to retrieve most borrowed books", ex);
        }
    }

    private static void ValidateInputParameters(GetMostBorrowedBooksQuery request)
    {
        if (request.StartDate > request.EndDate)
            throw new ValidationException("Start date cannot be greater than end date");

        if (request.StartDate > DateTime.UtcNow)
            throw new ValidationException("Start date cannot be in the future");

        if (request.EndDate > DateTime.UtcNow)
            throw new ValidationException("End date cannot be in the future");

        if (request.Page <= 0)
            throw new ValidationException("Page number must be greater than 0");

        if (request.PageSize <= 0 || request.PageSize > 100)
            throw new ValidationException("Page size must be between 1 and 100");
    }

    private static BooksAnalyticsResponse CreateEmptyResponse(GetMostBorrowedBooksQuery request)
    {
        return new BooksAnalyticsResponse
        {
            Books = new List<BookAnalyticsDto>(),
            TotalCount = 0,
            Page = request.Page,
            PageSize = request.PageSize,
            HasNextPage = false
        };
    }
}