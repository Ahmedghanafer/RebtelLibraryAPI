using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Queries.Borrowers;

public class GetBorrowerQueryHandler : IRequestHandler<GetBorrowerQuery, BorrowerDto?>
{
    private readonly IBorrowerRepository _borrowerRepository;
    private readonly ILogger<GetBorrowerQueryHandler> _logger;

    public GetBorrowerQueryHandler(
        IBorrowerRepository borrowerRepository,
        ILogger<GetBorrowerQueryHandler> logger)
    {
        _borrowerRepository = borrowerRepository ?? throw new ArgumentNullException(nameof(borrowerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BorrowerDto?> Handle(GetBorrowerQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            _logger.LogWarning("GetBorrowerQuery called with empty Id");
            return null;
        }

        try
        {
            var borrower = await _borrowerRepository.GetByIdAsync(request.Id, cancellationToken);

            if (borrower == null)
            {
                _logger.LogInformation("Borrower with Id {BorrowerId} not found", request.Id);
                return null;
            }

            _logger.LogInformation("Retrieved borrower with Id {BorrowerId}", request.Id);

            return MapToBorrowerDto(borrower);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrower with Id {BorrowerId}", request.Id);
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