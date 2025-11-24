using MediatR;
using RebtelLibraryAPI.Application.DTOs.Analytics;

namespace RebtelLibraryAPI.Application.Queries.Analytics;

public record EstimateReadingPaceQuery(
    Guid BorrowerId
) : IRequest<ReadingPaceResponse>;