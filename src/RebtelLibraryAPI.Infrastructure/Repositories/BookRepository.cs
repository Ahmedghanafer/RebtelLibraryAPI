using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Services;

namespace RebtelLibraryAPI.Infrastructure.Repositories;

/// <summary>
///     Entity Framework Core implementation of IBookRepository
/// </summary>
public class BookRepository : Repository<Book, Guid>, IBookRepository
{
    public BookRepository(
        LibraryDbContext context,
        ILogger<Repository<Book, Guid>> logger,
        DatabaseErrorHandler errorHandler) : base(context, logger, errorHandler)
    {
    }

    /// <inheritdoc />
    public async Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return null;

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ISBN == isbn, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Book>> GetByCategoryAsync(string category,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            return new List<Book>().AsReadOnly();

        return await _dbSet
            .AsNoTracking()
            .Where(b => b.Category == category)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.Availability == BookAvailability.Available)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsISBNUniqueAsync(string isbn, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        var query = _dbSet.Where(b => b.ISBN == isbn);

        if (excludeId.HasValue) query = query.Where(b => b.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Book>> GetBooksBorrowedByUsersAsync(IEnumerable<Guid> borrowerIds,
        CancellationToken cancellationToken = default)
    {
        if (borrowerIds == null || !borrowerIds.Any())
            return new List<Book>().AsReadOnly();

        // Get loans by the specified borrowers, then get the books they borrowed
        var borrowedBookIds = await _context.Loans
            .AsNoTracking()
            .Where(l => borrowerIds.Contains(l.BorrowerId) && l.Status == LoanStatus.Returned)
            .Select(l => l.BookId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!borrowedBookIds.Any())
            return new List<Book>().AsReadOnly();

        return await _dbSet
            .AsNoTracking()
            .Where(b => borrowedBookIds.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Searches books by title or author
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Books matching the search term</returns>
    public async Task<IReadOnlyList<Book>> SearchByTitleOrAuthorAsync(string searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Book>().AsReadOnly();

        var normalizedSearch = searchTerm.Trim().ToLowerInvariant();

        return await _dbSet
            .AsNoTracking()
            .Where(b =>
                b.Title.ToLower().Contains(normalizedSearch) ||
                b.Author.ToLower().Contains(normalizedSearch))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Gets books by availability status
    /// </summary>
    /// <param name="availability">The availability status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Books with the specified availability</returns>
    public async Task<IReadOnlyList<Book>> GetByAvailabilityAsync(BookAvailability availability,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.Availability == availability)
            .ToListAsync(cancellationToken);
    }
}