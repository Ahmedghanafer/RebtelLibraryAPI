using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Commands.Books;

public record CreateBookCommand(
    string Title,
    string Author,
    string ISBN,
    int PageCount,
    string Category
) : IRequest<BookDto>;