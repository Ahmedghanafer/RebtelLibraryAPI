using Microsoft.Extensions.Logging;
using RebtelLibraryAPI.Application.Queries.Borrowers;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Interfaces;

namespace RebtelLibraryAPI.UnitTests.Application.Queries.Borrowers;

public class GetBorrowerQueryHandlerTests
{
    private readonly Mock<IBorrowerRepository> _borrowerRepositoryMock;
    private readonly GetBorrowerQueryHandler _handler;
    private readonly Mock<ILogger<GetBorrowerQueryHandler>> _loggerMock;

    public GetBorrowerQueryHandlerTests()
    {
        _borrowerRepositoryMock = new Mock<IBorrowerRepository>();
        _loggerMock = new Mock<ILogger<GetBorrowerQueryHandler>>();
        _handler = new GetBorrowerQueryHandler(_borrowerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidBorrowerId_ShouldReturnBorrowerDto()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var borrower = Borrower.Create("John", "Doe", "john.doe@example.com", "555-555-0123");

        // Use reflection to set the ID to match our expected ID
        typeof(Entity<Guid>).GetProperty(nameof(Entity<Guid>.Id))!
            .SetValue(borrower, borrowerId);

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(borrower);

        var query = new GetBorrowerQuery(borrowerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(borrowerId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.Phone.Should().Be("5555550123");
        result.MemberStatus.Should().Be("Active");

        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentBorrowerId_ShouldReturnNull()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Borrower?)null);

        var query = new GetBorrowerQuery(borrowerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyBorrowerId_ShouldReturnNull()
    {
        // Arrange
        var query = new GetBorrowerQuery(Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var query = new GetBorrowerQuery(borrowerId);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, CancellationToken.None));

        // Assert
        exception.Should().Be(expectedException);
        _borrowerRepositoryMock.Verify(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveBorrower_ShouldReturnCorrectStatus()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var borrower = Borrower.Create("Jane", "Smith", "jane.smith@example.com");
        borrower.Deactivate();

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(borrower);

        var query = new GetBorrowerQuery(borrowerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MemberStatus.Should().Be("Inactive");
    }

    [Fact]
    public async Task Handle_WithBorrowerWithoutPhone_ShouldReturnEmptyPhone()
    {
        // Arrange
        var borrowerId = Guid.NewGuid();
        var borrower = Borrower.Create("Alice", "Johnson", "alice.johnson@example.com");

        _borrowerRepositoryMock
            .Setup(x => x.GetByIdAsync(borrowerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(borrower);

        var query = new GetBorrowerQuery(borrowerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Phone.Should().BeNull();
    }
}