using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Domain.Interfaces;

public interface IBookRepository : IRepository<Book, Guid>
{
    Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default);
    Task<bool> IsISBNUniqueAsync(string isbn, Guid? excludeId = null, CancellationToken cancellationToken = default);

    // Analytics methods
    Task<IReadOnlyList<Book>> GetBooksBorrowedByUsersAsync(IEnumerable<Guid> borrowerIds,
        CancellationToken cancellationToken = default);
}