using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;
using RebtelLibraryAPI.Domain.DTOs;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Services;

namespace RebtelLibraryAPI.Infrastructure.Repositories;

/// <summary>
///     Entity Framework Core implementation of ILoanRepository
/// </summary>
public class LoanRepository : Repository<Loan, Guid>, ILoanRepository
{
    public LoanRepository(
        LibraryDbContext context,
        ILogger<Repository<Loan, Guid>> logger,
        DatabaseErrorHandler errorHandler) : base(context, logger, errorHandler)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Loan>> GetActiveLoansForBorrowerAsync(Guid borrowerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BorrowerId == borrowerId && l.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Loan>> GetActiveLoansForBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BookId == bookId && l.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Loan>> GetOverdueLoansAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Loan?> GetActiveLoanForBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BookId == bookId && l.Status == LoanStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Loan>> GetLoanHistoryForBorrowerAsync(Guid borrowerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BorrowerId == borrowerId)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Gets loans by status
    /// </summary>
    /// <param name="status">The loan status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Loans with the specified status</returns>
    public async Task<IReadOnlyList<Loan>> GetByStatusAsync(LoanStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.Status == status)
            .OrderBy(l => l.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Gets loans due within the specified number of days
    /// </summary>
    /// <param name="days">Number of days from now</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Loans due within the specified period</returns>
    public async Task<IReadOnlyList<Loan>> GetLoansDueWithinDaysAsync(int days, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var dueDate = now.AddDays(days);

        return await _dbSet
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.Active && l.DueDate >= now && l.DueDate <= dueDate)
            .OrderBy(l => l.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Gets loans by date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Loans created within the date range</returns>
    public async Task<IReadOnlyList<Loan>> GetByBorrowDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BorrowDate >= startDate && l.BorrowDate <= endDate)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Gets the count of active loans for a borrower
    /// </summary>
    /// <param name="borrowerId">The borrower ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of active loans</returns>
    public async Task<int> GetActiveLoanCountForBorrowerAsync(Guid borrowerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(l => l.BorrowerId == borrowerId && l.Status == LoanStatus.Active, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Loan>> GetCompletedLoansByDateRangeAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.Returned &&
                       l.BorrowDate >= startDate &&
                       l.BorrowDate <= endDate &&
                       l.ReturnDate.HasValue)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Loan>> GetCompletedLoansForBorrowerAsync(Guid borrowerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BorrowerId == borrowerId &&
                       l.Status == LoanStatus.Returned &&
                       l.ReturnDate.HasValue)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetBorrowersWhoBorrowedBookAsync(Guid bookId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BookId == bookId && l.Status == LoanStatus.Returned)
            .Select(l => l.BorrowerId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MostBorrowedBookAnalytics>> GetMostBorrowedBooksAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.Returned &&
                       l.BorrowDate >= startDate &&
                       l.BorrowDate <= endDate)
            .Join(_context.Books,
                loan => loan.BookId,
                book => book.Id,
                (loan, book) => new { loan, book })
            .GroupBy(x => x.book)
            .Select(g => new MostBorrowedBookAnalytics
            {
                Id = g.Key.Id,
                Title = g.Key.Title,
                Author = g.Key.Author,
                ISBN = g.Key.ISBN,
                BorrowCount = g.Count(),
                PageCount = g.Key.PageCount,
                Category = g.Key.Category
            })
            .OrderByDescending(b => b.BorrowCount)
            .ThenBy(b => b.Title)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CompletedLoanWithBookAnalytics>> GetCompletedLoansWithBookForBorrowerAsync(Guid borrowerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BorrowerId == borrowerId &&
                       l.Status == LoanStatus.Returned &&
                       l.ReturnDate.HasValue)
            .Join(_context.Books,
                loan => loan.BookId,
                book => book.Id,
                (loan, book) => new CompletedLoanWithBookAnalytics
                {
                    LoanId = loan.Id,
                    BookId = loan.BookId,
                    BookTitle = book.Title,
                    BookAuthor = book.Author,
                    BookISBN = book.ISBN,
                    BookPageCount = book.PageCount,
                    BorrowDate = loan.BorrowDate,
                    ReturnDate = loan.ReturnDate
                })
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync(cancellationToken);
    }
}