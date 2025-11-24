using FluentAssertions;
using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.UnitTests.Domain;

public class InputSanitizationTests
{
    [Fact]
    public void Create_WithMaliciousInput_ShouldSanitizeInput()
    {
        // Act
        var borrower = Borrower.Create(
            "<script>alert('xss')</script>",
            "Doe&company",
            "test@example.com",
            "555-555-0123");

        // Assert - malicious characters should be removed
        borrower.FirstName.Should().Be("scriptalertxssscript");
        borrower.LastName.Should().Be("Doecompany");
        borrower.Email.Should().Be("test@example.com");
        borrower.Phone.Should().Be("5555550123");
    }

    [Fact]
    public void CreateFromFullName_WithMaliciousInput_ShouldSanitizeInput()
    {
        // Act
        var borrower = Borrower.CreateFromFullName(
            "<script>John</script> 'onload='alert('xss')&company <img>",
            "test@example.com");

        // Assert
        borrower.FirstName.Should().Be("scriptJohnscript");
        borrower.LastName.Should().Be("onloadalertxsscompany img");
    }

    [Fact]
    public void UpdateContactInfo_WithMaliciousInput_ShouldSanitizeInput()
    {
        // Arrange
        var borrower = Borrower.Create("John", "Doe", "john@example.com");

        // Act
        borrower.UpdateContactInfo(
            "<script>alert('hack')</script>",
            "Doe'&company=\"hacked\"",
            "555-555-0123");

        // Assert
        borrower.FirstName.Should().Be("scriptalerthackscript");
        borrower.LastName.Should().Be("Doecompanyhacked");
    }
}