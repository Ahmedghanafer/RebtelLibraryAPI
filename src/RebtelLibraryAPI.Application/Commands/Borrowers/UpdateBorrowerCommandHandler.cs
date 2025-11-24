using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Commands.Borrowers;

public class UpdateBorrowerCommandHandler : IRequestHandler<UpdateBorrowerCommand, BorrowerDto>
{
    private readonly IBorrowerRepository _borrowerRepository;
    private readonly ILogger<UpdateBorrowerCommandHandler> _logger;

    public UpdateBorrowerCommandHandler(
        IBorrowerRepository borrowerRepository,
        ILogger<UpdateBorrowerCommandHandler> logger)
    {
        _borrowerRepository = borrowerRepository ?? throw new ArgumentNullException(nameof(borrowerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BorrowerDto> Handle(UpdateBorrowerCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Updating borrower with ID: {BorrowerId}", request.Id);

        try
        {
            // Check if borrower exists
            var existingBorrower = await _borrowerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingBorrower == null)
            {
                _logger.LogWarning("Borrower with ID {BorrowerId} not found", request.Id);
                throw new BorrowerNotFoundException($"Borrower with ID {request.Id} not found");
            }

            // Handle email update with uniqueness check
            if (request.Email != null && request.Email != existingBorrower.Email)
            {
                // Domain validates email format first, then check uniqueness
                existingBorrower.UpdateEmail(request.Email.Trim()); // This will validate email format

                // Only check uniqueness if the domain validation passed
                var emailInUse = await _borrowerRepository.IsEmailUniqueAsync(request.Email, request.Id, cancellationToken);
                if (!emailInUse)
                {
                    _logger.LogWarning("Email {Email} is already in use by another borrower", request.Email);
                    throw new BorrowerValidationException("Email is already in use by another borrower");
                }
            }

            // Handle name update
            if (request.Name != null)
            {
                existingBorrower.UpdateContactInfoFromFullName(request.Name.Trim(), existingBorrower.Phone);
            }

            // Handle phone update (only if no name update, since that already handles phone)
            if (request.Phone != null && request.Name == null)
            {
                existingBorrower.UpdateContactInfo(
                    existingBorrower.FirstName,
                    existingBorrower.LastName,
                    request.Phone?.Trim());
            }

            // Handle activity status update
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    existingBorrower.Activate();
                }
                else
                {
                    existingBorrower.Deactivate();
                }
            }

            // Update the borrower
            var updatedBorrower = await _borrowerRepository.UpdateAsync(existingBorrower, cancellationToken);

            _logger.LogInformation("Successfully updated borrower with ID: {BorrowerId}", updatedBorrower.Id);

            return MapToBorrowerDto(updatedBorrower);
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating borrower with ID: {BorrowerId}", request.Id);
            throw new ValidationException("Failed to update borrower", ex);
        }
    }

    
    private static BorrowerDto MapToBorrowerDto(Borrower borrower)
    {
        if (borrower == null)
            throw new ArgumentNullException(nameof(borrower));

        return new BorrowerDto
        {
            Id = borrower.Id,
            FirstName = borrower.FirstName ?? string.Empty,
            LastName = borrower.LastName ?? string.Empty,
            Email = borrower.Email ?? string.Empty,
            Phone = borrower.Phone,
            RegistrationDate = borrower.RegistrationDate,
            MemberStatus = borrower.MemberStatus.ToString()
        };
    }
}