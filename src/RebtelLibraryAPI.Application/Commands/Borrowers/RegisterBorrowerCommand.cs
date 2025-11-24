using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Commands.Borrowers;

public record RegisterBorrowerCommand(
    string Name,
    string Email,
    string Phone
) : IRequest<BorrowerDto>;