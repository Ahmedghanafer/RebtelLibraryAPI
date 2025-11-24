namespace RebtelLibraryAPI.Application.DTOs;

public class BorrowerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string MemberStatus { get; set; } = string.Empty;
}