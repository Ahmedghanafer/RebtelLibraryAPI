using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using RebtelLibraryAPI.Application.Commands.Loans;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;
using Xunit;

namespace RebtelLibraryAPI.UnitTests.Application.Commands.Loans;

public class BorrowBookCommandHandlerTests
{
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IBorrowerRepository> _borrowerRepositoryMock;
    private readonly Mock<ILogger<BorrowBookCommandHandler>> _loggerMock;
    private readonly BorrowBookCommandHandler _handler;

    public BorrowBookCommandHandlerTests()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _borrowerRepositoryMock = new Mock<IBorrowerRepository>();
        _loggerMock = new Mock<ILogger<BorrowBookCommandHandler>>();

        _handler = new BorrowBookCommandHandler(
            _loanRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _borrowerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateLoanAndMarkBookAsBorrowed()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, borrowerId);

        var book = Book.Create(
            "Test Book",
            "Test Author",
            "1234567890",
            200,
            "Fiction");

        var borrower = Borrower.Create(
            "John",
            "Doe",
            "john.doe@example.com");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(borrower);

        _loanRepositoryMock
            .Setup(x => x.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var createdLoan = Loan.Create(bookId, borrowerId);
        _loanRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdLoan);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BookId.Should().Be(bookId);
        result.BorrowerId.Should().Be(borrowerId);
        result.Status.Should().Be("Active");

        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Book>(b => b.Availability == BookAvailability.Borrowed), It.IsAny<CancellationToken>()), Times.Once);
        _loanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BookNotFound_ShouldThrowBookNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, borrowerId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BookNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BookNotAvailable_ShouldThrowBookNotAvailableException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, borrowerId);

        var book = Book.Create(
            "Test Book",
            "Test Author",
            "1234567890",
            200,
            "Fiction");
        book.MarkAsBorrowed();

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act & Assert
        await Assert.ThrowsAsync<BookNotAvailableException>(
            () => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BorrowerNotFound_ShouldThrowBorrowerNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, borrowerId);

        var book = Book.Create(
            "Test Book",
            "Test Author",
            "1234567890",
            200,
            "Fiction");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BorrowerNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BorrowerNotActive_ShouldThrowBorrowerNotActiveException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, borrowerId);

        var book = Book.Create(
            "Test Book",
            "Test Author",
            "1234567890",
            200,
            "Fiction");

        var borrower = Borrower.Create(
            "John",
            "Doe",
            "john.doe@example.com");

        borrower.Deactivate();

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(borrower);

        // Act & Assert
        await Assert.ThrowsAsync<BorrowerNotActiveException>(
            () => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BookAlreadyHasActiveLoan_ShouldThrowBookNotAvailableException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, borrowerId);

        var book = Book.Create(
            "Test Book",
            "Test Author",
            "1234567890",
            200,
            "Fiction");

        var borrower = Borrower.Create(
            "John",
            "Doe",
            "john.doe@example.com");

        var existingLoan = Loan.Create(bookId, Guid.NewGuid());

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(borrower);

        _loanRepositoryMock
            .Setup(x => x.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        // Act & Assert
        await Assert.ThrowsAsync<BookNotAvailableException>(
            () => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}