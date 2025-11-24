using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Services;

namespace RebtelLibraryAPI.Infrastructure.Repositories;

/// <summary>
///     Entity Framework Core implementation of IBorrowerRepository
/// </summary>
public class BorrowerRepository : Repository<Borrower, Guid>, IBorrowerRepository
{
    public BorrowerRepository(
        LibraryDbContext context,
        ILogger<Repository<Borrower, Guid>> logger,
        DatabaseErrorHandler errorHandler) : base(context, logger, errorHandler)
    {
    }

    /// <inheritdoc />
    public async Task<Borrower?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Email == normalizedEmail, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.ToLowerInvariant().Trim();
        var query = _dbSet.Where(b => b.Email == normalizedEmail);

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Borrower>> GetActiveBorrowersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.MemberStatus == MemberStatus.Active)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Searches borrowers by name or email
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Borrowers matching the search term</returns>
    public async Task<IReadOnlyList<Borrower>> SearchByNameOrEmailAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Borrower>().AsReadOnly();

        var normalizedSearch = searchTerm.Trim().ToLowerInvariant();

        return await _dbSet
            .AsNoTracking()
            .Where(b =>
                b.FirstName.ToLower().Contains(normalizedSearch) ||
                b.LastName.ToLower().Contains(normalizedSearch) ||
                b.Email.ToLower().Contains(normalizedSearch))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Gets borrowers by member status
    /// </summary>
    /// <param name="status">The member status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Borrowers with the specified status</returns>
    public async Task<IReadOnlyList<Borrower>> GetByMemberStatusAsync(MemberStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.MemberStatus == status)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Gets borrowers by registration date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Borrowers registered within the date range</returns>
    public async Task<IReadOnlyList<Borrower>> GetByRegistrationDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.RegistrationDate >= startDate && b.RegistrationDate <= endDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Borrower> Borrowers, int TotalCount)> GetFilteredBorrowersAsync(
        string? searchTerm = null,
        MemberStatus? memberStatus = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        // Apply member status filter
        if (memberStatus.HasValue)
        {
            query = query.Where(b => b.MemberStatus == memberStatus.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(b =>
                b.FirstName.ToLower().Contains(normalizedSearch) ||
                b.LastName.ToLower().Contains(normalizedSearch) ||
                b.Email.ToLower().Contains(normalizedSearch) ||
                (b.Phone != null && b.Phone.Contains(normalizedSearch)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering and pagination
        var borrowers = await query
            .OrderBy(b => b.LastName)
            .ThenBy(b => b.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (borrowers, totalCount);
    }
}