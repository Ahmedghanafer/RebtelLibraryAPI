namespace RebtelLibraryAPI.Domain.DTOs;

public class CompletedLoanWithBookAnalytics
{
    public Guid LoanId { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string BookISBN { get; set; } = string.Empty;
    public int BookPageCount { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}