using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.UnitTests.Domain;

public class BorrowerTests
{
    [Fact]
    public void Create_ValidBorrower_ShouldCreateBorrower()
    {
        // Act
        var borrower = Borrower.Create("John", "Doe", "john@example.com", "555-555-0123");

        // Assert
        borrower.Should().NotBeNull();
        borrower.FirstName.Should().Be("John");
        borrower.LastName.Should().Be("Doe");
        borrower.Email.Should().Be("john@example.com");
        borrower.Phone.Should().Be("5555550123");
        borrower.MemberStatus.Should().Be(MemberStatus.Active);
    }

    [Fact]
    public void Create_EmptyFirstName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<BorrowerValidationException>(() =>
            Borrower.Create("", "Doe", "john@example.com"));
    }

    [Fact]
    public void Create_EmptyEmail_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<BorrowerValidationException>(() =>
            Borrower.Create("John", "Doe", ""));
    }

    [Fact]
    public void Create_InvalidEmail_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<BorrowerValidationException>(() =>
            Borrower.Create("John", "Doe", "invalid-email"));
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmail()
    {
        // Arrange
        var borrower = Borrower.Create("John", "Doe", "john@example.com");

        // Act
        borrower.UpdateEmail("newemail@example.com");

        // Assert
        borrower.Email.Should().Be("newemail@example.com");
    }

    [Fact]
    public void Suspend_ShouldUpdateMemberStatus()
    {
        // Arrange
        var borrower = Borrower.Create("John", "Doe", "john@example.com");

        // Act
        borrower.Suspend();

        // Assert
        borrower.MemberStatus.Should().Be(MemberStatus.Suspended);
        borrower.CanBorrowBooks().Should().BeFalse();
    }

    [Fact]
    public void GetFullName_ShouldReturnCombinedName()
    {
        // Arrange
        var borrower = Borrower.Create("John", "Doe", "john@example.com");

        // Act
        var fullName = borrower.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }
}