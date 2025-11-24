using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Queries.Books;

public record GetBookQuery(Guid Id) : IRequest<BookDto?>;