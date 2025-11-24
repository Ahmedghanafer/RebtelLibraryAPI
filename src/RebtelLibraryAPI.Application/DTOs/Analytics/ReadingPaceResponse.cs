namespace RebtelLibraryAPI.Application.DTOs.Analytics;

public class ReadingPaceResponse
{
    public Guid BorrowerId { get; set; }
    public decimal AveragePagesPerDay { get; set; }
    public int LoanCountUsed { get; set; }
    public bool HasSufficientData { get; set; }
    public string? Message { get; set; }
}