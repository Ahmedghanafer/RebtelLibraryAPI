using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.Commands.Loans;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Commands.Loans;

public class ReturnBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly ReturnBookCommandHandler _handler;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<ILogger<ReturnBookCommandHandler>> _loggerMock;

    public ReturnBookCommandHandlerTests()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<ReturnBookCommandHandler>>();

        _handler = new ReturnBookCommandHandler(
            _loanRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnBookAndUpdateAvailability()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId, borrowerId);

        var book = Book.Create(
            "Test Book",
            "Test Author",
            "1234567890",
            200,
            "Fiction");
        book.MarkAsBorrowed();

        var loan = Loan.Create(bookId, borrowerId);

        _loanRepositoryMock
            .Setup(x => x.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BookId.Should().Be(bookId);
        result.BorrowerId.Should().Be(borrowerId);
        result.Status.Should().BeOneOf("Returned", "Overdue");
        result.ReturnDate.Should().NotBeNull();

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Book>(b => b.Availability == BookAvailability.Available),
                It.IsAny<CancellationToken>()), Times.Once);
        _loanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoActiveLoanFound_ShouldThrowLoanNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId, borrowerId);

        _loanRepositoryMock
            .Setup(x => x.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<LoanNotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LongBelongsToDifferentBorrower_ShouldThrowLoanNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var differentBorrowerId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId, borrowerId);

        var loan = Loan.Create(bookId, differentBorrowerId);

        _loanRepositoryMock
            .Setup(x => x.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        // Act & Assert
        await Assert.ThrowsAsync<LoanNotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BookNotFound_ShouldThrowBookNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId, borrowerId);

        var loan = Loan.Create(bookId, borrowerId);

        _loanRepositoryMock
            .Setup(x => x.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BookNotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _loanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidReturn_ShouldReturnBookWithCorrectStatus()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId, borrowerId);

        var book = Book.Create(
            "Test Book",
            "Test Author",
            "1234567890",
            200,
            "Fiction");
        book.MarkAsBorrowed();

        var loan = Loan.Create(bookId, borrowerId);

        _loanRepositoryMock
            .Setup(x => x.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BookId.Should().Be(bookId);
        result.BorrowerId.Should().Be(borrowerId);
        result.Status.Should().BeOneOf("Returned", "Overdue");
        result.ReturnDate.Should().NotBeNull();

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Book>(b => b.Availability == BookAvailability.Available),
                It.IsAny<CancellationToken>()), Times.Once);
        _loanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}