using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RebtelLibraryAPI.Application.Commands.Loans;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Repositories;
using RebtelLibraryAPI.Infrastructure.Services;

namespace RebtelLibraryAPI.IntegrationTests.Application.Commands.Loans;

/// <summary>
///     Integration tests for loan command handlers with real database operations
/// </summary>
public class LoanCommandHandlerIntegrationTests : IDisposable
{
    private readonly IBookRepository _bookRepository;
    private readonly BorrowBookCommandHandler _borrowBookHandler;
    private readonly IBorrowerRepository _borrowerRepository;
    private readonly LibraryDbContext _context;
    private readonly ILoanRepository _loanRepository;
    private readonly ReturnBookCommandHandler _returnBookHandler;

    public LoanCommandHandlerIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new LibraryDbContext(options);

        var bookLogger = new Mock<ILogger<BookRepository>>().Object;
        var borrowerLogger = new Mock<ILogger<BorrowerRepository>>().Object;
        var loanLogger = new Mock<ILogger<LoanRepository>>().Object;
        var errorHandler = new Mock<DatabaseErrorHandler>(Mock.Of<ILogger<DatabaseErrorHandler>>()).Object;

        _bookRepository = new BookRepository(_context, bookLogger, errorHandler);
        _borrowerRepository = new BorrowerRepository(_context, borrowerLogger, errorHandler);
        _loanRepository = new LoanRepository(_context, loanLogger, errorHandler);

        var borrowLogger = new Mock<ILogger<BorrowBookCommandHandler>>().Object;
        var returnLogger = new Mock<ILogger<ReturnBookCommandHandler>>().Object;

        _borrowBookHandler = new BorrowBookCommandHandler(
            _loanRepository,
            _bookRepository,
            _borrowerRepository,
            borrowLogger);

        _returnBookHandler = new ReturnBookCommandHandler(
            _loanRepository,
            _bookRepository,
            returnLogger);
    }


    public void Dispose()
    {
        _context?.Dispose();
    }


    [Fact]
    public async Task ConcurrentBorrowingAttempts_ShouldPreventDoubleBorrowing()
    {
        // Arrange
        var book = Book.Create("Concurrency Test Book", "Test Author", "9781234567891", 200, "Fiction");
        var borrower1 = Borrower.Create("Borrower", "One", "borrower1@test.com");
        var borrower2 = Borrower.Create("Borrower", "Two", "borrower2@test.com");

        await _bookRepository.AddAsync(book);
        await _borrowerRepository.AddAsync(borrower1);
        await _borrowerRepository.AddAsync(borrower2);

        // Act - First borrow should succeed
        var firstBorrowCommand = new BorrowBookCommand(book.Id, borrower1.Id);
        var firstLoanResult = await _borrowBookHandler.Handle(firstBorrowCommand, CancellationToken.None);

        // Assert
        firstLoanResult.Should().NotBeNull();

        // Act - Second borrow should fail
        var secondBorrowCommand = new BorrowBookCommand(book.Id, borrower2.Id);
        Func<Task> secondBorrowAction = async () =>
            await _borrowBookHandler.Handle(secondBorrowCommand, CancellationToken.None);

        // Assert
        await secondBorrowAction.Should().ThrowAsync<BookNotAvailableException>();
    }

    [Fact]
    public async Task LoanCreation_WithStandardPeriod_ShouldSetCorrectDueDate()
    {
        // Arrange
        var book = Book.Create("Period Test Book", "Test Author", "9781234567892", 250, "Fiction");
        var borrower = Borrower.Create("Period", "Test User", "period@test.com");

        await _bookRepository.AddAsync(book);
        await _borrowerRepository.AddAsync(borrower);

        // Act
        var borrowCommand = new BorrowBookCommand(book.Id, borrower.Id);
        var loanResult = await _borrowBookHandler.Handle(borrowCommand, CancellationToken.None);

        // Assert
        loanResult.Should().NotBeNull();
        loanResult.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromMinutes(1));

        // Verify the loan entity in database
        var loanEntity = await _loanRepository.GetByIdAsync(loanResult.Id);
        loanEntity.Should().NotBeNull();
        loanEntity!.Status.Should().Be(LoanStatus.Active);
        loanEntity.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task LoanTransactionIntegrity_ShouldMaintainConsistency()
    {
        // Arrange
        var book = Book.Create("Transaction Test Book", "Test Author", "9781234567893", 180, "Fiction");
        var borrower = Borrower.Create("Transaction", "Test User", "transaction@test.com");

        await _bookRepository.AddAsync(book);
        await _borrowerRepository.AddAsync(borrower);

        var initialBookState = await _bookRepository.GetByIdAsync(book.Id);
        var initialLoanCount = (await _loanRepository.GetAllAsync()).Count();

        // Act
        var borrowCommand = new BorrowBookCommand(book.Id, borrower.Id);
        var loanResult = await _borrowBookHandler.Handle(borrowCommand, CancellationToken.None);

        // Assert - Check database consistency
        var finalBookState = await _bookRepository.GetByIdAsync(book.Id);
        var finalLoanCount = (await _loanRepository.GetAllAsync()).Count();

        finalBookState!.Availability.Should().Be(BookAvailability.Borrowed);
        finalLoanCount.Should().Be(initialLoanCount + 1);

        // Verify loan entity exists in database
        var loanEntity = await _loanRepository.GetByIdAsync(loanResult.Id);
        loanEntity.Should().NotBeNull();
        loanEntity!.BookId.Should().Be(book.Id);
        loanEntity.BorrowerId.Should().Be(borrower.Id);
    }
}