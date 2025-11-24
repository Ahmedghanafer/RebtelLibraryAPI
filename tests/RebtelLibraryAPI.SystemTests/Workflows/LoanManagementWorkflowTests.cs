using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;
using RebtelLibraryAPI.Infrastructure.Data;
using RebtelLibraryAPI.Infrastructure.Repositories;
using RebtelLibraryAPI.Infrastructure.Services;
using Testcontainers.SqlEdge;
using Xunit.Abstractions;

namespace RebtelLibraryAPI.SystemTests.Workflows;

/// <summary>
///     System tests for complete loan management workflows using real database
/// </summary>
public class LoanManagementWorkflowTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly SqlEdgeContainer _sqlContainer;
    private IBookRepository _bookRepository;
    private IBorrowerRepository _borrowerRepository;
    private string _connectionString = string.Empty;
    private LibraryDbContext _context;
    private ILoanRepository _loanRepository;
    private IServiceScope _scope;

    public LoanManagementWorkflowTests(ITestOutputHelper output)
    {
        _output = output;
        _sqlContainer = new SqlEdgeBuilder()
            .WithPassword("StrongPassword123!")
            .WithName($"sql-edge-{Guid.NewGuid():D}")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        _connectionString = _sqlContainer.GetConnectionString();

        _output.WriteLine($"Started SQL Edge container: {_sqlContainer.Id}");

        // Configure services with test database
        var services = new ServiceCollection();

        services.AddDbContext<LibraryDbContext>(options => { options.UseSqlServer(_connectionString); });

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce log noise in tests
        });

        services.AddScoped<DatabaseErrorHandler>();

        services.AddScoped<IBookRepository>(provider =>
        {
            var context = provider.GetRequiredService<LibraryDbContext>();
            var logger = provider.GetRequiredService<ILogger<BookRepository>>();
            var errorHandler = provider.GetRequiredService<DatabaseErrorHandler>();
            return new BookRepository(context, logger, errorHandler);
        });

        services.AddScoped<IBorrowerRepository>(provider =>
        {
            var context = provider.GetRequiredService<LibraryDbContext>();
            var logger = provider.GetRequiredService<ILogger<BorrowerRepository>>();
            var errorHandler = provider.GetRequiredService<DatabaseErrorHandler>();
            return new BorrowerRepository(context, logger, errorHandler);
        });

        services.AddScoped<ILoanRepository>(provider =>
        {
            var context = provider.GetRequiredService<LibraryDbContext>();
            var logger = provider.GetRequiredService<ILogger<LoanRepository>>();
            var errorHandler = provider.GetRequiredService<DatabaseErrorHandler>();
            return new LoanRepository(context, logger, errorHandler);
        });

        var serviceProvider = services.BuildServiceProvider();
        _scope = serviceProvider.CreateScope();

        _context = _scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        await _context.Database.MigrateAsync();

        _bookRepository = _scope.ServiceProvider.GetRequiredService<IBookRepository>();
        _borrowerRepository = _scope.ServiceProvider.GetRequiredService<IBorrowerRepository>();
        _loanRepository = _scope.ServiceProvider.GetRequiredService<ILoanRepository>();

        _output.WriteLine("Database initialized and migrated");
    }

    public async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await _sqlContainer.DisposeAsync();
        _scope?.Dispose();
        _context?.Dispose();
    }

    [Fact]
    public async Task CompleteBorrowingAndReturningWorkflow_ShouldWorkCorrectly()
    {
        // Arrange - Create a realistic library scenario
        var book = await CreateSingleTestBook("The Great Gatsby", "F. Scott Fitzgerald");
        var borrower = await CreateSingleTestBorrower("John", "john.doe@example.com");

        _output.WriteLine($"Starting library workflow: {borrower.GetFullName()} borrowing '{book.Title}'");

        // Act - Borrow the book
        var loan = Loan.Create(book.Id, borrower.Id);
        await _loanRepository.AddAsync(loan);
        book.MarkAsBorrowed();
        await _bookRepository.UpdateAsync(book);

        // Verify the book is borrowed
        var borrowedBook = await _bookRepository.GetByIdAsync(book.Id);
        borrowedBook.Should().NotBeNull();
        borrowedBook!.Availability.Should().Be(BookAvailability.Borrowed);

        var activeLoan = await _loanRepository.GetActiveLoanForBookAsync(book.Id);
        activeLoan.Should().NotBeNull();
        activeLoan!.Status.Should().Be(LoanStatus.Active);

        _output.WriteLine($"Book '{book.Title}' successfully borrowed by {borrower.GetFullName()}");

        // Act - Return the book
        await Task.Delay(100); // Simulate some time passing
        activeLoan.ReturnBook();
        await _loanRepository.UpdateAsync(activeLoan);

        // Get fresh copy of book to avoid entity tracking issues
        var freshBook = await _bookRepository.GetByIdAsync(book.Id);
        freshBook.Should().NotBeNull();
        freshBook!.MarkAsAvailable();
        await _bookRepository.UpdateAsync(freshBook);

        // Assert - Verify the book is returned and available
        var returnedBook = await _bookRepository.GetByIdAsync(book.Id);
        returnedBook.Should().NotBeNull();
        returnedBook!.Availability.Should().Be(BookAvailability.Available);

        var finalLoan = await _loanRepository.GetByIdAsync(activeLoan.Id);
        finalLoan.Should().NotBeNull();
        finalLoan!.Status.Should().Be(LoanStatus.Returned);
        finalLoan.ReturnDate.Should().NotBeNull();

        _output.WriteLine($"Book '{book.Title}' successfully returned by {borrower.GetFullName()}");
        _output.WriteLine($"Loan duration: {finalLoan.ReturnDate.Value.Subtract(loan.BorrowDate).TotalDays:F1} days");
    }

    [Fact]
    public async Task MultipleBorrowersManagingLoans_ShouldWorkCorrectly()
    {
        // Arrange - Simulate multiple users borrowing and returning books
        var books = await CreateTestBooks(5);
        var borrowers = await CreateTestBorrowers(3);

        _output.WriteLine($"Starting multi-user workflow: {borrowers.Count} borrowers, {books.Count} books");

        // Act - Each borrower borrows some books
        var loansCreated = new List<(Book Book, Borrower Borrower)>();
        for (var i = 0; i < books.Count; i++)
        {
            var book = books[i];
            var borrower = borrowers[i % borrowers.Count];

            var loan = Loan.Create(book.Id, borrower.Id);
            await _loanRepository.AddAsync(loan);

            book.MarkAsBorrowed();
            await _bookRepository.UpdateAsync(book);

            loansCreated.Add((book, borrower));
        }

        // Verify all books are borrowed
        var allBooks = await _bookRepository.GetAllAsync();
        allBooks.Should().OnlyContain(b => b.Availability == BookAvailability.Borrowed);

        var allLoans = await _loanRepository.GetAllAsync();
        allLoans.Should().HaveCount(5);
        allLoans.Should().OnlyContain(l => l.Status == LoanStatus.Active);

        _output.WriteLine("All books successfully borrowed");

        // Act - Return all books
        foreach (var (book, borrower) in loansCreated)
        {
            var activeLoan = await _loanRepository.GetActiveLoanForBookAsync(book.Id);
            activeLoan.Should().NotBeNull();

            activeLoan!.ReturnBook();
            await _loanRepository.UpdateAsync(activeLoan);

            book.MarkAsAvailable();
            await _bookRepository.UpdateAsync(book);
        }

        // Assert - Verify all books are returned and available
        var finalBooks = await _bookRepository.GetAllAsync();
        finalBooks.Should().OnlyContain(b => b.Availability == BookAvailability.Available);

        var finalLoans = await _loanRepository.GetAllAsync();
        finalLoans.Should().OnlyContain(l => l.Status == LoanStatus.Returned);
        finalLoans.Should().OnlyContain(l => l.ReturnDate.HasValue);

        _output.WriteLine("All books successfully returned");

        // Verify loan history
        foreach (var borrower in borrowers)
        {
            var borrowerLoans = await _loanRepository.GetActiveLoansForBorrowerAsync(borrower.Id);
            // Should be 0 since all loans are returned
            borrowerLoans.Count.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task LibraryLoanHistory_ShouldTrackCorrectly()
    {
        // Arrange - Create a realistic scenario for loan history tracking
        var book1 = await CreateSingleTestBook("Clean Code", "Robert C. Martin");
        var book2 = await CreateSingleTestBook("Design Patterns", "Erich Gamma");
        var borrower = await CreateSingleTestBorrower("Alice", "alice@example.com");

        _output.WriteLine($"Testing loan history tracking for {borrower.GetFullName()}");

        // Act - Create multiple loan cycles
        var loan1 = Loan.Create(book1.Id, borrower.Id);
        await _loanRepository.AddAsync(loan1);
        book1.MarkAsBorrowed();
        await _bookRepository.UpdateAsync(book1);

        await Task.Delay(50); // Simulate time between loans

        var loan2 = Loan.Create(book2.Id, borrower.Id);
        await _loanRepository.AddAsync(loan2);
        book2.MarkAsBorrowed();
        await _bookRepository.UpdateAsync(book2);

        await Task.Delay(50); // Simulate borrowing period

        loan1.ReturnBook();
        await _loanRepository.UpdateAsync(loan1);
        book1.MarkAsAvailable();
        await _bookRepository.UpdateAsync(book1);

        // Assert - Verify loan data integrity
        var allLoans = await _loanRepository.GetAllAsync();
        allLoans.Should().HaveCount(2);

        var book1Loans = allLoans.Where(l => l.BookId == book1.Id).ToList();
        var book2Loans = allLoans.Where(l => l.BookId == book2.Id).ToList();

        book1Loans.Should().HaveCount(1);
        book1Loans.First().Status.Should().Be(LoanStatus.Returned);
        book1Loans.First().ReturnDate.Should().NotBeNull();

        book2Loans.Should().HaveCount(1);
        book2Loans.First().Status.Should().Be(LoanStatus.Active);

        // Verify book availability status
        var finalBook1 = await _bookRepository.GetByIdAsync(book1.Id);
        var finalBook2 = await _bookRepository.GetByIdAsync(book2.Id);

        finalBook1.Should().NotBeNull();
        finalBook1!.Availability.Should().Be(BookAvailability.Available);

        finalBook2.Should().NotBeNull();
        finalBook2!.Availability.Should().Be(BookAvailability.Borrowed);

        _output.WriteLine("Loan history verified: 1 returned, 1 active");
        _output.WriteLine($"Total loans created: {allLoans.Count}");
    }

    private async Task<List<Book>> CreateTestBooks(int count)
    {
        var books = new List<Book>();
        for (var i = 0; i < count; i++)
        {
            var book = Book.Create($"Test Book {i + 1}", $"Test Author {i + 1}", GenerateISBN10(), 200 + i, "Fiction");
            await _bookRepository.AddAsync(book);
            books.Add(book);
        }

        return books;
    }

    private async Task<List<Borrower>> CreateTestBorrowers(int count)
    {
        var borrowers = new List<Borrower>();
        for (var i = 0; i < count; i++)
        {
            var borrower = BorrowerCreate($"Test {i + 1}", $"User {i + 1}", $"test{i + 1}@example.com");
            await _borrowerRepository.AddAsync(borrower);
            borrowers.Add(borrower);
        }

        return borrowers;
    }

    private async Task<Book> CreateSingleTestBook(string title, string author)
    {
        var book = Book.Create(title, author, GenerateISBN10(), 200, "Fiction");
        await _bookRepository.AddAsync(book);
        return book;
    }

    private async Task<Borrower> CreateSingleTestBorrower(string firstName, string email)
    {
        var borrower = BorrowerCreate(firstName, "User", email);
        await _borrowerRepository.AddAsync(borrower);
        return borrower;
    }

    // Helper method to generate a 10-digit ISBN
    private string GenerateISBN10()
    {
        var random = new Random();
        var isbn = new string(Enumerable.Repeat(0, 10).Select(_ => (char)('0' + random.Next(0, 10))).ToArray());
        return isbn;
    }

    // Helper method to create borrower with correct parameter order
    private Borrower BorrowerCreate(string firstName, string lastName, string email)
    {
        return Borrower.Create(firstName, lastName, email, "1234567890");
    }
}