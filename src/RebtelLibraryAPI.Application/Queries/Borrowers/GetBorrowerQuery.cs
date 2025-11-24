using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Queries.Borrowers;

public record GetBorrowerQuery(Guid Id) : IRequest<BorrowerDto?>;