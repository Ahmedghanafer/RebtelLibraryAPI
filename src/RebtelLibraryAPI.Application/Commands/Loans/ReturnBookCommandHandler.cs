using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Commands.Loans;

public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, LoanDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<ReturnBookCommandHandler> _logger;

    public ReturnBookCommandHandler(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        ILogger<ReturnBookCommandHandler> logger)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoanDto> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing return request for book {BookId} by borrower {BorrowerId}",
            request.BookId, request.BorrowerId);

        try
        {
            // Find active loan for the book and borrower combination
            var activeLoan = await _loanRepository.GetActiveLoanForBookAsync(request.BookId, cancellationToken);
            if (activeLoan == null)
            {
                _logger.LogWarning("No active loan found for book {BookId}", request.BookId);
                throw new LoanNotFoundException($"No active loan found for book {request.BookId}");
            }

            if (activeLoan.BorrowerId != request.BorrowerId)
            {
                _logger.LogWarning(
                    "Loan {LoanId} belongs to different borrower. Expected: {BorrowerId}, Actual: {ActualBorrowerId}",
                    activeLoan.Id, request.BorrowerId, activeLoan.BorrowerId);
                throw new LoanNotFoundException("Loan belongs to different borrower");
            }

            // Get the book to update availability
            var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
            if (book == null)
            {
                _logger.LogWarning("Book not found: {BookId}", request.BookId);
                throw new BookNotFoundException($"Book with ID {request.BookId} not found");
            }

            // Return the book using domain business logic
            activeLoan.ReturnBook();

            // Mark book as available again
            book.MarkAsAvailable();

            // Update both loan and book in the repository
            await _loanRepository.UpdateAsync(activeLoan, cancellationToken);
            await _bookRepository.UpdateAsync(book, cancellationToken);

            _logger.LogInformation("Successfully returned book {BookId} and updated loan {LoanId}",
                request.BookId, activeLoan.Id);

            return MapToLoanDto(activeLoan);
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning book {BookId} by borrower {BorrowerId}",
                request.BookId, request.BorrowerId);
            throw new ValidationException("Failed to return book", ex);
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

public class LoanNotFoundException : DomainException
{
    public LoanNotFoundException(string message) : base(message, "LOAN_NOT_FOUND")
    {
    }

    public LoanNotFoundException(string message, Exception innerException)
        : base(message, "LOAN_NOT_FOUND", innerException)
    {
    }
}