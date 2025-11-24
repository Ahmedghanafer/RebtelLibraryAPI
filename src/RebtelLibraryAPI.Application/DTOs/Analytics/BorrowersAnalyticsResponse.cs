namespace RebtelLibraryAPI.Application.DTOs.Analytics;

public class BorrowersAnalyticsResponse
{
    public List<BorrowerAnalyticsDto> Borrowers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
}

public class BorrowerAnalyticsDto
{
    public Guid Id { get; set; }
    public int BorrowCount { get; set; }
}