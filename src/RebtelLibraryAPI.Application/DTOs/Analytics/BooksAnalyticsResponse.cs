namespace RebtelLibraryAPI.Application.DTOs.Analytics;

public class BooksAnalyticsResponse
{
    public List<BookAnalyticsDto> Books { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
}

public class BookAnalyticsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
    public int PageCount { get; set; }
    public string Category { get; set; } = string.Empty;
}