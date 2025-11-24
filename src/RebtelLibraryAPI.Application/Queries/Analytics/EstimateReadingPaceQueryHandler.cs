using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs.Analytics;
using RebtelLibraryAPI.Domain.DTOs;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Analytics;

public class EstimateReadingPaceQueryHandler : IRequestHandler<EstimateReadingPaceQuery, ReadingPaceResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<EstimateReadingPaceQueryHandler> _logger;

    public EstimateReadingPaceQueryHandler(
        ILoanRepository loanRepository,
        ILogger<EstimateReadingPaceQueryHandler> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadingPaceResponse> Handle(EstimateReadingPaceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Estimating reading pace for borrower {BorrowerId}", request.BorrowerId);

        try
        {
            if (request.BorrowerId == Guid.Empty)
                throw new ValidationException("Borrower ID cannot be empty");

            // Get completed loans for the borrower (includes Book data)
            var completedLoans = await _loanRepository.GetCompletedLoansWithBookForBorrowerAsync(
                request.BorrowerId, cancellationToken);

            if (!completedLoans.Any())
            {
                _logger.LogInformation("No completed loans found for borrower {BorrowerId}", request.BorrowerId);
                return CreateInsufficientDataResponse(request.BorrowerId, "No completed loans found for this borrower");
            }

            // Calculate reading pace for each completed loan
            var readingPaces = new List<decimal>();

            foreach (var loan in completedLoans)
            {
                if (loan.BookPageCount <= 0)
                    continue;

                var daysSpent = CalculateDaysSpent(loan);
                if (daysSpent > 0)
                {
                    var pagesPerDay = (decimal)loan.BookPageCount / daysSpent;
                    readingPaces.Add(pagesPerDay);

                    _logger.LogDebug("Loan {LoanId}: {PageCount} pages over {DaysSpent} days = {PagesPerDay:F2} pages/day",
                        loan.LoanId, loan.BookPageCount, daysSpent, pagesPerDay);
                }
            }

            if (!readingPaces.Any())
            {
                _logger.LogInformation("No valid reading pace calculations for borrower {BorrowerId}", request.BorrowerId);
                return CreateInsufficientDataResponse(request.BorrowerId, "Unable to calculate reading pace from completed loans");
            }

            // Calculate average reading pace
            var averagePagesPerDay = readingPaces.Average();

            var response = new ReadingPaceResponse
            {
                BorrowerId = request.BorrowerId,
                AveragePagesPerDay = Math.Round(averagePagesPerDay, 2),
                LoanCountUsed = readingPaces.Count,
                HasSufficientData = true,
                Message = $"Reading pace calculated from {readingPaces.Count} completed loans"
            };

            _logger.LogInformation("Calculated reading pace for borrower {BorrowerId}: {AveragePagesPerDay:F2} pages/day from {LoanCount} loans",
                request.BorrowerId, response.AveragePagesPerDay, response.LoanCountUsed);

            return response;
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating reading pace for borrower {BorrowerId}", request.BorrowerId);
            throw new ValidationException("Failed to estimate reading pace", ex);
        }
    }

    private static int CalculateDaysSpent(CompletedLoanWithBookAnalytics loan)
    {
        if (!loan.ReturnDate.HasValue)
            return 0;

        var returnDate = loan.ReturnDate.Value;
        var borrowDate = loan.BorrowDate;

        // Handle edge case: same-day returns
        if (returnDate.Date == borrowDate.Date)
        {
            // For same-day returns, assume 1 day to avoid division by zero
            return 1;
        }

        // Calculate the difference in days
        var daysSpent = (returnDate.Date - borrowDate.Date).Days;

        // Ensure at least 1 day to avoid division by zero
        return Math.Max(1, daysSpent);
    }

    private static ReadingPaceResponse CreateInsufficientDataResponse(Guid borrowerId, string message)
    {
        return new ReadingPaceResponse
        {
            BorrowerId = borrowerId,
            AveragePagesPerDay = 0,
            LoanCountUsed = 0,
            HasSufficientData = false,
            Message = message
        };
    }
}