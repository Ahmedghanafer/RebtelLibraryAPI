using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Commands.Borrowers;

public class RegisterBorrowerCommandHandler : IRequestHandler<RegisterBorrowerCommand, BorrowerDto>
{
    private readonly IBorrowerRepository _borrowerRepository;
    private readonly ILogger<RegisterBorrowerCommandHandler> _logger;

    public RegisterBorrowerCommandHandler(
        IBorrowerRepository borrowerRepository,
        ILogger<RegisterBorrowerCommandHandler> logger)
    {
        _borrowerRepository = borrowerRepository ?? throw new ArgumentNullException(nameof(borrowerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BorrowerDto> Handle(RegisterBorrowerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new borrower with email: {Email}", request.Email);

        try
        {
            // Check if email already exists
            var existingBorrower = await _borrowerRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingBorrower != null)
            {
                _logger.LogWarning("Borrower with email {Email} already exists", request.Email);
                throw new BorrowerValidationException("A borrower with this email already exists");
            }

            // Create the borrower using domain factory method (handles all validation)
            var borrower = Borrower.CreateFromFullName(request.Name, request.Email, request.Phone);

            // Add to repository
            var addedBorrower = await _borrowerRepository.AddAsync(borrower, cancellationToken);

            _logger.LogInformation("Successfully registered borrower with ID: {BorrowerId}", addedBorrower.Id);

            return MapToBorrowerDto(addedBorrower);
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering borrower with email: {Email}", request.Email);
            throw new ValidationException("Failed to register borrower", ex);
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