using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Commands.Loans;

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, LoanDto>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowerRepository _borrowerRepository;
    private readonly ILogger<BorrowBookCommandHandler> _logger;

    public BorrowBookCommandHandler(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IBorrowerRepository borrowerRepository,
        ILogger<BorrowBookCommandHandler> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _borrowerRepository = borrowerRepository ?? throw new ArgumentNullException(nameof(borrowerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoanDto> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing borrow request for book {BookId} by borrower {BorrowerId}",
            request.BookId, request.BorrowerId);

        try
        {
            // Validate book exists and is available
            var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
            if (book == null)
            {
                _logger.LogWarning("Book not found: {BookId}", request.BookId);
                throw new BookNotFoundException($"Book with ID {request.BookId} not found");
            }

            if (book.Availability != BookAvailability.Available)
            {
                _logger.LogWarning("Book not available: {BookId}, current status: {Availability}",
                    request.BookId, book.Availability);
                throw new BookNotAvailableException($"Book with ID {request.BookId} is not available for borrowing");
            }

            // Validate borrower exists and is active
            var borrower = await _borrowerRepository.GetByIdAsync(request.BorrowerId, cancellationToken);
            if (borrower == null)
            {
                _logger.LogWarning("Borrower not found: {BorrowerId}", request.BorrowerId);
                throw new BorrowerNotFoundException($"Borrower with ID {request.BorrowerId} not found");
            }

            if (borrower.MemberStatus != MemberStatus.Active)
            {
                _logger.LogWarning("Borrower not active: {BorrowerId}, current status: {MemberStatus}",
                    request.BorrowerId, borrower.MemberStatus);
                throw new BorrowerNotActiveException($"Borrower with ID {request.BorrowerId} is not active");
            }

            // Check if book already has an active loan (double-borrowing prevention)
            var existingActiveLoan = await _loanRepository.GetActiveLoanForBookAsync(request.BookId, cancellationToken);
            if (existingActiveLoan != null)
            {
                _logger.LogWarning("Book already has active loan: {BookId}, existing loan: {LoanId}",
                    request.BookId, existingActiveLoan.Id);
                throw new BookNotAvailableException($"Book with ID {request.BookId} is already borrowed");
            }

            // Create the loan using domain factory method
            var loan = Loan.Create(request.BookId, request.BorrowerId);

            // Mark book as borrowed
            book.MarkAsBorrowed();

            // Add loan to repository and update book status
            var addedLoan = await _loanRepository.AddAsync(loan, cancellationToken);
            await _bookRepository.UpdateAsync(book, cancellationToken);

            _logger.LogInformation("Successfully created loan {LoanId} for book {BookId} by borrower {BorrowerId}",
                addedLoan.Id, request.BookId, request.BorrowerId);

            return MapToLoanDto(addedLoan);
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error borrowing book {BookId} by borrower {BorrowerId}",
                request.BookId, request.BorrowerId);
            throw new ValidationException("Failed to borrow book", ex);
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