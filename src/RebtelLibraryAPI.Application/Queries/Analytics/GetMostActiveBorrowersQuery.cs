using MediatR;
using RebtelLibraryAPI.Application.DTOs.Analytics;

namespace RebtelLibraryAPI.Application.Queries.Analytics;

public record GetMostActiveBorrowersQuery(
    DateTime StartDate,
    DateTime EndDate,
    int Page = 1,
    int PageSize = 10
) : IRequest<BorrowersAnalyticsResponse>;