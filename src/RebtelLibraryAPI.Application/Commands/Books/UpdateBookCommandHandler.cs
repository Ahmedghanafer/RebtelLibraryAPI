using MediatR;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.Application.Commands.Books;

public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, BookDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<UpdateBookCommandHandler> _logger;

    public UpdateBookCommandHandler(
        IBookRepository bookRepository,
        ILogger<UpdateBookCommandHandler> logger)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookDto> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating book with ID: {BookId}", request.Id);

        try
        {
            // Check if book exists
            var existingBook = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingBook == null)
            {
                _logger.LogWarning("Book with ID {BookId} not found", request.Id);
                throw new BookNotFoundException($"Book with ID {request.Id} not found");
            }

            // Update book details (ISBN is immutable)
            existingBook.UpdateDetails(
                request.Title,
                request.Author,
                request.PageCount,
                request.Category
            );

            // Update availability status
            var newAvailability = request.IsAvailable ? BookAvailability.Available : BookAvailability.Borrowed;
            existingBook.UpdateAvailability(newAvailability);

            // Update the book
            var updatedBook = await _bookRepository.UpdateAsync(existingBook, cancellationToken);

            _logger.LogInformation("Successfully updated book with ID: {BookId}", updatedBook.Id);

            return MapToBookDto(updatedBook);
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book with ID: {BookId}", request.Id);
            throw new ValidationException("Failed to update book", ex);
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