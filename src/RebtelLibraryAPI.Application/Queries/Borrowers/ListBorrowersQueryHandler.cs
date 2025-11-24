using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Borrowers;

public class ListBorrowersQueryHandler : IRequestHandler<ListBorrowersQuery, ListBorrowersDto>
{
    private readonly IBorrowerRepository _borrowerRepository;
    private readonly ILogger<ListBorrowersQueryHandler> _logger;

    public ListBorrowersQueryHandler(
        IBorrowerRepository borrowerRepository,
        ILogger<ListBorrowersQueryHandler> logger)
    {
        _borrowerRepository = borrowerRepository ?? throw new ArgumentNullException(nameof(borrowerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListBorrowersDto> Handle(ListBorrowersQuery request, CancellationToken cancellationToken)
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

            // Parse member status filter
            MemberStatus? memberStatus = null;
            if (!string.IsNullOrWhiteSpace(request.MemberStatusFilter))
            {
                if (Enum.TryParse<MemberStatus>(request.MemberStatusFilter, true, out var status))
                {
                    memberStatus = status;
                    _logger.LogInformation("Filtering borrowers with status {Status}", request.MemberStatusFilter);
                }
                else
                {
                    // Invalid status filter, return empty result
                    _logger.LogWarning("Invalid member status filter: {Status}", request.MemberStatusFilter);
                    return new ListBorrowersDto
                    {
                        Borrowers = new List<BorrowerDto>(),
                        TotalCount = 0,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    };
                }
            }

            // Use database-level filtering for better performance
            var (borrowers, totalCount) = await _borrowerRepository.GetFilteredBorrowersAsync(
                request.SearchTerm,
                memberStatus,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                _logger.LogInformation("Applied search filter for term: {SearchTerm}", request.SearchTerm);

            var pagedBorrowers = borrowers.Select(MapToBorrowerDto).ToList();

            _logger.LogInformation(
                "Returning {BorrowerCount} borrowers, page {PageNumber} of {TotalPages}",
                pagedBorrowers.Count,
                request.PageNumber,
                (int)Math.Ceiling((double)totalCount / request.PageSize));

            return new ListBorrowersDto
            {
                Borrowers = pagedBorrowers,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrowers list");
            throw;
        }
    }

    private static BorrowerDto MapToBorrowerDto(Borrower borrower)
    {
        return new BorrowerDto
        {
            Id = borrower.Id,
            FirstName = borrower.FirstName,
            LastName = borrower.LastName,
            Email = borrower.Email,
            Phone = borrower.Phone,
            RegistrationDate = borrower.RegistrationDate,
            MemberStatus = borrower.MemberStatus.ToString()
        };
    }
}