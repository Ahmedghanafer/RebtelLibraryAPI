using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.DTOs;

namespace RebtelLibraryAPI.Domain.Interfaces;

public interface ILoanRepository : IRepository<Loan, Guid>
{
    Task<IReadOnlyList<Loan>> GetActiveLoansForBorrowerAsync(Guid borrowerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Loan>> GetActiveLoansForBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> GetOverdueLoansAsync(CancellationToken cancellationToken = default);
    Task<Loan?> GetActiveLoanForBookAsync(Guid bookId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Loan>> GetLoanHistoryForBorrowerAsync(Guid borrowerId,
        CancellationToken cancellationToken = default);

    // Analytics methods
    Task<IReadOnlyList<Loan>> GetCompletedLoansByDateRangeAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> GetCompletedLoansForBorrowerAsync(Guid borrowerId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetBorrowersWhoBorrowedBookAsync(Guid bookId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MostBorrowedBookAnalytics>> GetMostBorrowedBooksAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompletedLoanWithBookAnalytics>> GetCompletedLoansWithBookForBorrowerAsync(Guid borrowerId,
        CancellationToken cancellationToken = default);
}