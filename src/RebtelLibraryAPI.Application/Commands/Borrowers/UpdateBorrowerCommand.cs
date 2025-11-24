using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Commands.Borrowers;

public record UpdateBorrowerCommand(
    Guid Id,
    string? Name = null,
    string? Email = null,
    string? Phone = null,
    bool? IsActive = null
) : IRequest<BorrowerDto>;