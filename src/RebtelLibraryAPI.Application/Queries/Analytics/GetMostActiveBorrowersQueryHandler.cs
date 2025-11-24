using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs.Analytics;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Analytics;

public class
    GetMostActiveBorrowersQueryHandler : IRequestHandler<GetMostActiveBorrowersQuery, BorrowersAnalyticsResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<GetMostActiveBorrowersQueryHandler> _logger;

    public GetMostActiveBorrowersQueryHandler(
        ILoanRepository loanRepository,
        ILogger<GetMostActiveBorrowersQueryHandler> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BorrowersAnalyticsResponse> Handle(GetMostActiveBorrowersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting most active borrowers from {StartDate} to {EndDate}, page {Page}, size {PageSize}",
            request.StartDate, request.EndDate, request.Page, request.PageSize);

        try
        {
            // Validate input parameters
            ValidateInputParameters(request);

            // Get completed loans within the date range
            var loans = await _loanRepository.GetCompletedLoansByDateRangeAsync(
                request.StartDate, request.EndDate, cancellationToken);

            if (!loans.Any())
            {
                _logger.LogInformation("No completed loans found in the specified date range");
                return CreateEmptyResponse(request);
            }

            // Group by borrower and count borrow occurrences (privacy-focused)
            var borrowerAnalytics = loans
                .GroupBy(l => l.BorrowerId)
                .Select(g => new BorrowerAnalyticsDto
                {
                    Id = g.Key,
                    BorrowCount = g.Count()
                })
                .OrderByDescending(b => b.BorrowCount)
                .ThenBy(b => b.Id)
                .ToList();

            // Apply pagination
            var totalCount = borrowerAnalytics.Count;
            var pagedBorrowers = borrowerAnalytics
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var response = new BorrowersAnalyticsResponse
            {
                Borrowers = pagedBorrowers,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                HasNextPage = request.Page * request.PageSize < totalCount
            };

            _logger.LogInformation("Retrieved {BorrowerCount} most active borrowers (total: {TotalCount})",
                response.Borrowers.Count, totalCount);

            return response;
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving most active borrowers from {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);
            throw new ValidationException("Failed to retrieve most active borrowers", ex);
        }
    }

    private static void ValidateInputParameters(GetMostActiveBorrowersQuery request)
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

    private static BorrowersAnalyticsResponse CreateEmptyResponse(GetMostActiveBorrowersQuery request)
    {
        return new BorrowersAnalyticsResponse
        {
            Borrowers = new List<BorrowerAnalyticsDto>(),
            TotalCount = 0,
            Page = request.Page,
            PageSize = request.PageSize,
            HasNextPage = false
        };
    }
}