using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.Commands.Borrowers;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Commands.Borrowers;

public class RegisterBorrowerCommandHandlerTests
{
    private readonly Mock<IBorrowerRepository> _borrowerRepositoryMock;
    private readonly RegisterBorrowerCommandHandler _handler;
    private readonly Mock<ILogger<RegisterBorrowerCommandHandler>> _loggerMock;

    public RegisterBorrowerCommandHandlerTests()
    {
        _borrowerRepositoryMock = new Mock<IBorrowerRepository>();
        _loggerMock = new Mock<ILogger<RegisterBorrowerCommandHandler>>();
        _handler = new RegisterBorrowerCommandHandler(
            _borrowerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateBorrowerAndReturnBorrowerDto()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "John Doe",
            "john.doe@example.com",
            "555-123-4567");

        var createdBorrower = Borrower.Create(
            "John",
            "Doe",
            "john.doe@example.com",
            "5551234567");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        _borrowerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdBorrower.Id, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal("5551234567", result.Phone);
        Assert.Equal("Active", result.MemberStatus);

        _borrowerRepositoryMock.Verify(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        _borrowerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldThrowBorrowerValidationException()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "Jane Smith",
            "jane.smith@example.com",
            "555-987-6543");

        var existingBorrower = Borrower.Create(
            "Jane",
            "Smith",
            "jane.smith@example.com",
            "5559876543");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("already exists", exception.Message);
        Assert.Equal("BORROWER_VALIDATION_ERROR", exception.ErrorCode);

        _borrowerRepositoryMock.Verify(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        _borrowerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")] // Empty email
    [InlineData(null)] // Null email
    [InlineData("invalid-email")] // Invalid format
    [InlineData("no-at-symbol.com")] // Missing @
    [InlineData("@domain.com")] // Missing local part
    [InlineData("user@")] // Missing domain
    [InlineData("user@domain")] // Missing TLD
    public async Task Handle_InvalidEmail_ShouldThrowBorrowerValidationException(string email)
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "Test User",
            email!,
            "555-123-4567");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        if (string.IsNullOrEmpty(email))
            Assert.Contains("required", exception.Message);
        else
            Assert.Contains("format", exception.Message);
    }

    [Theory]
    [InlineData("")] // Empty name
    [InlineData(null)] // Null name
    [InlineData("   ")] // Whitespace only
    public async Task Handle_InvalidName_ShouldThrowBorrowerValidationException(string name)
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            name!,
            "test@example.com",
            "555-123-4567");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("required", exception.Message);
    }

    [Theory]
    [InlineData("123-456-7890", "1234567890")] // Format with dashes
    [InlineData("(123) 456-7890", "1234567890")] // Format with parentheses
    [InlineData("1234567890", "1234567890")] // Already digits only
    [InlineData("+1 (123) 456-7890", "11234567890")] // With country code
    public async Task Handle_ValidPhoneFormats_ShouldNormalizeAndCreateBorrower(string phoneInput, string expectedPhone)
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "John Doe",
            "john.doe@example.com",
            phoneInput);

        var createdBorrower = Borrower.Create(
            "John",
            "Doe",
            "john.doe@example.com",
            expectedPhone);

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        _borrowerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPhone, result.Phone);
    }

    [Theory]
    [InlineData("123456789")] // Too short
    [InlineData("1234567890123456")] // Too long
    public async Task Handle_InvalidPhoneLength_ShouldThrowBorrowerValidationException(string phone)
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "John Doe",
            "john.doe@example.com",
            phone);

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Phone number", exception.Message);
        Assert.Contains("digits", exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyPhone_ShouldCreateBorrowerWithNullPhone()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "John Doe",
            "john.doe@example.com",
            "");

        var createdBorrower = Borrower.Create(
            "John",
            "Doe",
            "john.doe@example.com");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        _borrowerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Phone);
    }

    [Theory]
    [InlineData("John", "Doe", "John Doe")] // First and last name
    [InlineData("John", "", "John")] // First name only
    [InlineData("John", "Robert Smith", "John Robert Smith")] // Multiple last names
    public async Task Handle_SingleName_ShouldSetLastNameToEmpty(string expectedFirstName, string expectedLastName,
        string inputName)
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            inputName,
            "john.doe@example.com",
            "555-123-4567");

        var createdBorrower = Borrower.Create(
            expectedFirstName,
            expectedLastName,
            "john.doe@example.com",
            "5551234567");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        _borrowerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedFirstName, result.FirstName);
        Assert.Equal(expectedLastName, result.LastName);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldThrowValidationException()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "John Doe",
            "john.doe@example.com",
            "555-123-4567");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        _borrowerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Failed to register borrower", exception.Message);
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
    }

    [Fact]
    public async Task Handle_CaseInsensitiveEmail_ShouldNormalizeToLowercase()
    {
        // Arrange
        var command = new RegisterBorrowerCommand(
            "John Doe",
            "John.Doe@EXAMPLE.COM",
            "555-123-4567");

        var createdBorrower = Borrower.Create(
            "John",
            "Doe",
            "john.doe@example.com",
            "5551234567");

        _borrowerRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        _borrowerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("john.doe@example.com", result.Email);
    }
}