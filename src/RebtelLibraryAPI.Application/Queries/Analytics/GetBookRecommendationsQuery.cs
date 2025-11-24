using MediatR;
using RebtelLibraryAPI.Application.DTOs.Analytics;

namespace RebtelLibraryAPI.Application.Queries.Analytics;

public record GetBookRecommendationsQuery(
    Guid BookId,
    int Limit = 5
) : IRequest<BooksAnalyticsResponse>;