using RebtelLibraryAPI.Domain.Events;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.Domain.Entities;

public class Loan : Entity<Guid>
{
    private Loan(
        Guid id,
        Guid bookId,
        Guid borrowerId,
        DateTime borrowDate,
        DateTime dueDate,
        LoanStatus status
    ) : base(id)
    {
        BookId = bookId;
        BorrowerId = borrowerId;
        BorrowDate = borrowDate;
        DueDate = dueDate;
        Status = status;
    }

    // Parameterless constructor for EF Core
    private Loan() : base()
    {
        Status = LoanStatus.Active;
        BorrowDate = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(14);
    }

    public Guid BookId { get; }
    public Guid BorrowerId { get; }
    public DateTime BorrowDate { get; }
    public DateTime? ReturnDate { get; private set; }
    public DateTime DueDate { get; }
    public LoanStatus Status { get; private set; }

    public static Loan Create(Guid bookId, Guid borrowerId, int loanPeriodDays = 14)
    {
        ValidateLoanPeriod(loanPeriodDays);

        var loanId = Guid.NewGuid();
        var borrowDate = DateTime.UtcNow;
        var dueDate = borrowDate.AddDays(loanPeriodDays);

        var loan = new Loan(loanId, bookId, borrowerId, borrowDate, dueDate, LoanStatus.Active);
        loan.AddDomainEvent(new BookBorrowedEvent(loanId, bookId, borrowerId));

        return loan;
    }

    public void ReturnBook(DateTime? returnDate = null)
    {
        if (Status != LoanStatus.Active)
            throw new LoanOperationException("Only active loans can be returned");

        var actualReturnDate = returnDate ?? DateTime.UtcNow;

        if (actualReturnDate < BorrowDate)
            throw new LoanOperationException("Return date cannot be before borrow date");

        ReturnDate = actualReturnDate;
        Status = actualReturnDate > DueDate ? LoanStatus.Overdue : LoanStatus.Returned;

        UpdateTimestamp();
        AddDomainEvent(new BookReturnedEvent(Id, BookId, BorrowerId));
    }

    public void MarkAsOverdue()
    {
        if (Status != LoanStatus.Active)
            throw new LoanOperationException("Only active loans can be marked as overdue");

        if (DateTime.UtcNow <= DueDate)
            throw new LoanOperationException("Loan is not yet overdue");

        Status = LoanStatus.Overdue;
        UpdateTimestamp();
    }

    public bool IsOverdue()
    {
        return Status == LoanStatus.Active && DateTime.UtcNow > DueDate;
    }

    public bool IsReturned()
    {
        return Status == LoanStatus.Returned || Status == LoanStatus.Overdue;
    }

    public bool IsActive()
    {
        return Status == LoanStatus.Active;
    }

    public int DaysOverdue()
    {
        if (!IsOverdue() && Status != LoanStatus.Overdue)
            return 0;

        var overdueDate = DateTime.UtcNow > DueDate ? DateTime.UtcNow : DueDate;
        return (int)(overdueDate - DueDate).TotalDays;
    }

    public decimal CalculateOverdueFee(decimal dailyOverdueFee = 0.50m)
    {
        if (Status != LoanStatus.Overdue)
            return 0m;

        var overdueDays = (ReturnDate?.Date ?? DateTime.UtcNow.Date) - DueDate.Date;
        return Math.Max(0, overdueDays.Days) * dailyOverdueFee;
    }


    private static void ValidateLoanPeriod(int loanPeriodDays)
    {
        if (loanPeriodDays <= 0)
            throw new LoanValidationException("Loan period must be greater than 0 days");

        if (loanPeriodDays > 365)
            throw new LoanValidationException("Loan period cannot exceed 365 days");

        const int standardLoanPeriod = 14;
        const int maxAllowedVariation = 28;

        if (Math.Abs(loanPeriodDays - standardLoanPeriod) > maxAllowedVariation)
            throw new LoanValidationException(
                $"Loan period must be between {standardLoanPeriod - maxAllowedVariation} and {standardLoanPeriod + maxAllowedVariation} days");
    }
}

public enum LoanStatus
{
    Active,
    Returned,
    Overdue
}

public class LoanValidationException : DomainException
{
    public LoanValidationException(string message) : base(message, "LOAN_VALIDATION_ERROR")
    {
    }

    public LoanValidationException(string message, Exception innerException)
        : base(message, "LOAN_VALIDATION_ERROR", innerException)
    {
    }
}

public class LoanOperationException : DomainException
{
    public LoanOperationException(string message) : base(message, "LOAN_OPERATION_ERROR")
    {
    }

    public LoanOperationException(string message, Exception innerException)
        : base(message, "LOAN_OPERATION_ERROR", innerException)
    {
    }
}