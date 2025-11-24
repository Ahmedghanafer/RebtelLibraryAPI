using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Queries.Books;

public record ListBooksQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? CategoryFilter = null,
    string? SearchTerm = null
) : IRequest<ListBooksDto>;