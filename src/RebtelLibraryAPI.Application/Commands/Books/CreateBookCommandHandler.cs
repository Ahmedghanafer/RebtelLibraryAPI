using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Commands.Books;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, BookDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<CreateBookCommandHandler> _logger;

    public CreateBookCommandHandler(
        IBookRepository bookRepository,
        ILogger<CreateBookCommandHandler> logger)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookDto> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new book with ISBN: {ISBN}", request.ISBN);

        try
        {
            // Check if ISBN already exists
            var existingBook = await _bookRepository.GetByISBNAsync(request.ISBN, cancellationToken);
            if (existingBook != null)
            {
                _logger.LogWarning("Book with ISBN {ISBN} already exists", request.ISBN);
                throw new BookExistsException("A book with this ISBN already exists");
            }

            // Create the book using domain factory method
            var book = Book.Create(
                request.Title,
                request.Author,
                request.ISBN,
                request.PageCount,
                request.Category
            );

            // Add to repository
            var addedBook = await _bookRepository.AddAsync(book, cancellationToken);

            _logger.LogInformation("Successfully created book with ID: {BookId}", addedBook.Id);

            return MapToBookDto(addedBook);
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book with ISBN: {ISBN}", request.ISBN);
            throw new ValidationException("Failed to create book", ex);
        }
    }

    private static BookDto MapToBookDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PageCount = book.PageCount,
            Category = book.Category,
            Availability = book.Availability.ToString(),
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };
    }
}