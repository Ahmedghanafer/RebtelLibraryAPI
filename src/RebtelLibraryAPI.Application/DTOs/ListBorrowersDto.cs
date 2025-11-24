namespace RebtelLibraryAPI.Application.DTOs;

public class ListBorrowersDto
{
    public IReadOnlyList<BorrowerDto> Borrowers { get; set; } = new List<BorrowerDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}