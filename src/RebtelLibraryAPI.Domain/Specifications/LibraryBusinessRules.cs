using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.Domain.Specifications;

public static class LibraryBusinessRules
{
    private static readonly BookMustBeAvailable BookMustBeAvailable = new();
    private static readonly BorrowerMustBeActive BorrowerMustBeActive = new();
    private static readonly LoanMustBeActive LoanMustBeActive = new();

    public static void ValidateBookBorrowing(Book book, Borrower borrower)
    {
        if (!BookMustBeAvailable.IsSatisfiedBy(book))
            throw new LibraryBusinessRuleException(BookMustBeAvailable.ErrorMessage);

        if (!BorrowerMustBeActive.IsSatisfiedBy(borrower))
            throw new LibraryBusinessRuleException(BorrowerMustBeActive.ErrorMessage);
    }

    public static void ValidateBookReturning(Loan loan, Book book, Borrower borrower)
    {
        if (!LoanMustBeActive.IsSatisfiedBy(loan))
            throw new LibraryBusinessRuleException("Only active loans can be returned");

        if (loan.BookId != book.Id)
            throw new LibraryBusinessRuleException("Loan does not belong to this book");

        if (loan.BorrowerId != borrower.Id)
            throw new LibraryBusinessRuleException("Loan does not belong to this borrower");
    }

    public static bool CanBorrowerBorrowBook(Borrower borrower, IReadOnlyList<Loan> activeLoans)
    {
        if (!BorrowerMustBeActive.IsSatisfiedBy(borrower))
            return false;

        const int MaxActiveLoans = 5;
        return activeLoans.Count < MaxActiveLoans;
    }

    public static bool IsBookAvailableForBorrowing(Book book, IReadOnlyList<Loan> activeLoans)
    {
        if (!BookMustBeAvailable.IsSatisfiedBy(book))
            return false;

        return !activeLoans.Any(loan => loan.BookId == book.Id && loan.IsActive());
    }

    public static void ValidateLoanExtension(Loan loan, TimeSpan extensionPeriod)
    {
        if (!LoanMustBeActive.IsSatisfiedBy(loan))
            throw new LibraryBusinessRuleException("Only active loans can be extended");

        const int MaxExtensionDays = 14;
        if (extensionPeriod.TotalDays > MaxExtensionDays)
            throw new LibraryBusinessRuleException($"Loan cannot be extended by more than {MaxExtensionDays} days");

        const int MaxTotalLoanDays = 42;
        var newDueDate = loan.DueDate + extensionPeriod;
        var totalLoanDays = (newDueDate - loan.BorrowDate).TotalDays;

        if (totalLoanDays > MaxTotalLoanDays)
            throw new LibraryBusinessRuleException($"Total loan period cannot exceed {MaxTotalLoanDays} days");
    }
}

public class LibraryBusinessRuleException : DomainException
{
    public LibraryBusinessRuleException(string message) : base(message, "BUSINESS_RULE_VIOLATION")
    {
    }

    public LibraryBusinessRuleException(string message, Exception innerException)
        : base(message, "BUSINESS_RULE_VIOLATION", innerException)
    {
    }
}