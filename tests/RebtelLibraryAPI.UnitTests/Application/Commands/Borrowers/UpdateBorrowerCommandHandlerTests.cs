using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.Commands.Borrowers;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Commands.Borrowers;

public class UpdateBorrowerCommandHandlerTests
{
    private readonly Mock<IBorrowerRepository> _borrowerRepositoryMock;
    private readonly UpdateBorrowerCommandHandler _handler;
    private readonly Mock<ILogger<UpdateBorrowerCommandHandler>> _loggerMock;

    public UpdateBorrowerCommandHandlerTests()
    {
        _borrowerRepositoryMock = new Mock<IBorrowerRepository>();
        _loggerMock = new Mock<ILogger<UpdateBorrowerCommandHandler>>();
        _handler = new UpdateBorrowerCommandHandler(
            _borrowerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidNameUpdate_ShouldUpdateBorrowerName()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            "Jane Smith");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidEmailUpdate_ShouldUpdateBorrowerEmail()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            Email: "jane.smith@example.com");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.IsEmailUniqueAsync(command.Email, borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _borrowerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _borrowerRepositoryMock.Verify(
            x => x.IsEmailUniqueAsync(command.Email, borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidPhoneUpdate_ShouldUpdateBorrowerPhone()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            Phone: "(123) 456-7890");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_StatusUpdate_ShouldUpdateBorrowerStatus()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            IsActive: false);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentBorrower_ShouldThrowBorrowerNotFoundException()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var command = new UpdateBorrowerCommand(
            borrowerId,
            "New Name");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("not found", exception.Message);
        Assert.Equal("BORROWER_NOT_FOUND", exception.ErrorCode);

        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")] // Empty email
    [InlineData("invalid-email")] // Invalid format
    [InlineData("@domain.com")] // Missing local part
    [InlineData("user@")] // Missing domain
    public async Task Handle_InvalidEmailUpdate_ShouldThrowBorrowerValidationException(string email)
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            Email: email);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        if (string.IsNullOrEmpty(email))
            Assert.Contains("required", exception.Message);
        else
            Assert.Contains("format", exception.Message);

        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateEmailUpdate_ShouldThrowBorrowerValidationException()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            Email: "jane.smith@example.com");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.IsEmailUniqueAsync(command.Email, borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("already in use", exception.Message);

        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(
            x => x.IsEmailUniqueAsync(command.Email, borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("123456789")] // Too short
    [InlineData("1234567890123456")] // Too long
    public async Task Handle_InvalidPhoneUpdate_ShouldThrowBorrowerValidationException(string phone)
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            Phone: phone);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Phone number", exception.Message);

        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")] // Empty name
    [InlineData("   ")] // Whitespace only
    public async Task Handle_InvalidNameUpdate_ShouldThrowBorrowerValidationException(string name)
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            name);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BorrowerValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("required", exception.Message);

        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SameEmailUpdate_ShouldNotCheckUniqueness()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var email = "john.doe@example.com";
        var existingBorrower = Borrower.Create("John", "Doe", email, "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            Email: email);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(
            x => x.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleFieldUpdates_ShouldUpdateAllFields()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            "Jane Smith",
            "jane.smith@example.com",
            "(123) 456-7890",
            false);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.IsEmailUniqueAsync(command.Email, borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _borrowerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(
            x => x.IsEmailUniqueAsync(command.Email, borrowerId, It.IsAny<CancellationToken>()), Times.Once);
        _borrowerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldThrowValidationException()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var existingBorrower = Borrower.Create("John", "Doe", "john.doe@example.com", "5551234567");

        var command = new UpdateBorrowerCommand(
            borrowerId,
            "New Name");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBorrower);

        _borrowerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Failed to update borrower", exception.Message);
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
    }
}