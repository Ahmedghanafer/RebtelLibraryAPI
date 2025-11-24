using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Services;

namespace RebtelLibraryAPI.Infrastructure.Repositories;

/// <summary>
///     Base repository implementation using Entity Framework Core
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
/// <typeparam name="TKey">The key type</typeparam>
public abstract class Repository<T, TKey> : IRepository<T, TKey> where T : Entity<TKey>
{
    protected readonly LibraryDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<Repository<T, TKey>> _logger;
    protected readonly DatabaseErrorHandler _errorHandler;
    protected readonly string _entityName;

    protected Repository(LibraryDbContext context, ILogger<Repository<T, TKey>> logger, DatabaseErrorHandler errorHandler)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
        _errorHandler = errorHandler;
        _entityName = typeof(T).Name;
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _errorHandler.LogSuccessfulOperation(_entityName, "added", entity.Id as Guid?);
            return entity;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleDatabaseException(ex, _entityName, "add");
            throw; // This line should never be reached as HandleDatabaseException always throws
        }
    }

    /// <inheritdoc />
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _errorHandler.LogSuccessfulOperation(_entityName, "updated", entity.Id as Guid?);
            return entity;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleDatabaseException(ex, _entityName, "update");
            throw; // This line should never be reached as HandleDatabaseException always throws
        }
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            try
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);

                _errorHandler.LogSuccessfulOperation(_entityName, "deleted", entity.Id as Guid?);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleDatabaseException(ex, _entityName, "delete");
                throw; // This line should never be reached as HandleDatabaseException always throws
            }
        }
    }

    /// <summary>
    ///     Gets the count of entities in the repository
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The count of entities</returns>
    protected virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    ///     Checks if any entity exists matching the specified predicate
    /// </summary>
    /// <param name="predicate">The predicate to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entity matches, false otherwise</returns>
    protected virtual async Task<bool> AnyAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }
}