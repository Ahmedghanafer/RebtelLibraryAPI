using FluentAssertions;
using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.UnitTests.Domain;

public class LoanTests
{
    [Fact]
    public void Create_ValidLoan_ShouldCreateLoan()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();

        // Act
        var loan = Loan.Create(bookId, borrowerId);

        // Assert
        loan.Should().NotBeNull();
        loan.BookId.Should().Be(bookId);
        loan.BorrowerId.Should().Be(borrowerId);
        loan.Status.Should().Be(LoanStatus.Active);
        loan.ReturnDate.Should().BeNull();
    }

    [Fact]
    public void ReturnBook_ShouldMarkAsReturned()
    {
        // Arrange
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        loan.ReturnBook();

        // Assert
        loan.Status.Should().Be(LoanStatus.Returned);
        loan.ReturnDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ReturnBook_AlreadyReturned_ShouldThrowException()
    {
        // Arrange
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());
        loan.ReturnBook();

        // Act & Assert
        Assert.Throws<LoanOperationException>(() => loan.ReturnBook());
    }

    [Fact]
    public void IsActive_ShouldReturnCorrectStatus()
    {
        // Arrange
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        loan.IsActive().Should().BeTrue();

        // Act
        loan.ReturnBook();

        // Assert
        loan.IsActive().Should().BeFalse();
    }

    [Fact]
    public void Create_CustomLoanPeriod_ShouldUseCustomPeriod()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var customPeriod = 21;

        // Act
        var loan = Loan.Create(bookId, borrowerId, customPeriod);

        // Assert
        loan.DueDate.Should().BeCloseTo(loan.BorrowDate.AddDays(customPeriod), TimeSpan.FromSeconds(1));
    }
}