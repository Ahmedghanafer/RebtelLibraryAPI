using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Commands.Books;

public record UpdateBookCommand(
    Guid Id,
    string Title,
    string Author,
    int PageCount,
    string Category,
    bool IsAvailable
) : IRequest<BookDto>;