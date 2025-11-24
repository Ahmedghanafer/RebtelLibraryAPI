namespace RebtelLibraryAPI.Application.DTOs;

public class LoanDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid BorrowerId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
    public decimal OverdueFee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ListLoansResponse
{
    public List<LoanDto> Loans { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
}