using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Domain.Interfaces;

public interface IBorrowerRepository : IRepository<Borrower, Guid>
{
    Task<Borrower?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrower>> GetActiveBorrowersAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Borrower> Borrowers, int TotalCount)> GetFilteredBorrowersAsync(
        string? searchTerm = null,
        MemberStatus? memberStatus = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}