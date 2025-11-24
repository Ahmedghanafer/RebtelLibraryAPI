using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Queries.Borrowers;

public record ListBorrowersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? MemberStatusFilter = null
) : IRequest<ListBorrowersDto>;