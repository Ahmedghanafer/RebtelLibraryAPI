using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Queries.Loans;

public record GetActiveLoansQuery(
    Guid BorrowerId,
    int Page = 1,
    int PageSize = 10
) : IRequest<ListLoansResponse>;