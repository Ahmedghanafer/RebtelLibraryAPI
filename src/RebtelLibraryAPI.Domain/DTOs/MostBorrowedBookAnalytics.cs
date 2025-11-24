namespace RebtelLibraryAPI.Domain.DTOs;

public class MostBorrowedBookAnalytics
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
    public int PageCount { get; set; }
    public string Category { get; set; } = string.Empty;
}