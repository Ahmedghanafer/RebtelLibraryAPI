using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Loans;

public class GetActiveLoansQueryHandler : IRequestHandler<GetActiveLoansQuery, ListLoansResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<GetActiveLoansQueryHandler> _logger;

    public GetActiveLoansQueryHandler(
        ILoanRepository loanRepository,
        ILogger<GetActiveLoansQueryHandler> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListLoansResponse> Handle(GetActiveLoansQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving active loans for borrower {BorrowerId}, page {Page}, size {PageSize}",
            request.BorrowerId, request.Page, request.PageSize);

        try
        {
            // Validate pagination parameters
            if (request.Page <= 0)
                throw new ValidationException("Page number must be greater than 0");

            if (request.PageSize <= 0 || request.PageSize > 100)
                throw new ValidationException("Page size must be between 1 and 100");

            // Get active loans for the borrower
            var activeLoans = await _loanRepository.GetActiveLoansForBorrowerAsync(request.BorrowerId, cancellationToken);

            // Map to DTOs and calculate overdue status
            var loanDtos = activeLoans
                .Select(loan => MapToLoanDto(loan))
                .ToList();

            // Apply pagination
            var totalCount = loanDtos.Count;
            var pagedLoans = loanDtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var response = new ListLoansResponse
            {
                Loans = pagedLoans,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                HasNextPage = (request.Page * request.PageSize) < totalCount
            };

            _logger.LogInformation("Retrieved {LoanCount} active loans for borrower {BorrowerId} (total: {TotalCount})",
                response.Loans.Count, request.BorrowerId, totalCount);

            return response;
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active loans for borrower {BorrowerId}", request.BorrowerId);
            throw new ValidationException("Failed to retrieve active loans", ex);
        }
    }

    private static LoanDto MapToLoanDto(Loan loan)
    {
        return new LoanDto
        {
            Id = loan.Id,
            BookId = loan.BookId,
            BorrowerId = loan.BorrowerId,
            BorrowDate = loan.BorrowDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Status = loan.Status.ToString(),
            IsOverdue = loan.IsOverdue(),
            DaysOverdue = loan.DaysOverdue(),
            OverdueFee = loan.CalculateOverdueFee(),
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };
    }
}