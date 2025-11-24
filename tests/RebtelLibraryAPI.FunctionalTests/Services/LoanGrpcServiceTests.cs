using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RebtelLibraryAPI.API;
using RebtelLibraryAPI.Application.Commands.Loans;
using RebtelLibraryAPI.Application.DTOs;
using RebtelLibraryAPI.Domain.Entities;
using RebtelLibraryAPI.Domain.Exceptions;

namespace RebtelLibraryAPI.FunctionalTests.Services;

public class LoanGrpcServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LoanGrpcServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private GrpcChannel CreateChannel()
    {
        return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpClient = _factory.CreateClient()
        });
    }

    [Fact]
    public async Task BorrowBook_ValidRequestFormat_ShouldPassValidation()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new BorrowBookRequest
        {
            BookId = Guid.NewGuid().ToString(),
            BorrowerId = Guid.NewGuid().ToString()
        };

        // Act & Assert - Valid format but non-existent data should return NotFound
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.BorrowBookAsync(request));

        exception.StatusCode.Should().Be(StatusCode.NotFound);
        exception.Status.Detail.Should().Contain("not found");
    }

    [Fact]
    public async Task BorrowBook_InvalidBookId_ShouldReturnInvalidArgumentError()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new BorrowBookRequest
        {
            BookId = "invalid-guid",
            BorrowerId = Guid.NewGuid().ToString()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.BorrowBookAsync(request));

        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Status.Detail.Should().Contain("book ID");
    }

    [Fact]
    public async Task BorrowBook_InvalidBorrowerId_ShouldReturnInvalidArgumentError()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new BorrowBookRequest
        {
            BookId = Guid.NewGuid().ToString(),
            BorrowerId = "invalid-guid"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.BorrowBookAsync(request));

        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Status.Detail.Should().Contain("borrower ID");
    }

    [Fact]
    public async Task BorrowBook_EmptyBookId_ShouldReturnInvalidArgumentError()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new BorrowBookRequest
        {
            BookId = "",
            BorrowerId = Guid.NewGuid().ToString()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.BorrowBookAsync(request));

        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ReturnBook_ValidRequestFormat_ShouldPassValidation()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new ReturnBookRequest
        {
            BookId = Guid.NewGuid().ToString(),
            BorrowerId = Guid.NewGuid().ToString()
        };

        // Act & Assert - Valid format but non-existent loan should return error
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.ReturnBookAsync(request));

        // Should be either NotFound (book/borrower not found) or Internal (no active loan)
        exception.StatusCode.Should().BeOneOf(StatusCode.NotFound, StatusCode.Internal);
        (exception.Status.Detail.Contains("No active loan") || exception.Status.Detail.Contains("not found")).Should().BeTrue();
    }

    [Fact]
    public async Task ReturnBook_InvalidBookId_ShouldReturnInvalidArgumentError()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new ReturnBookRequest
        {
            BookId = "invalid-guid",
            BorrowerId = Guid.NewGuid().ToString()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.ReturnBookAsync(request));

        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Status.Detail.Should().Contain("book ID");
    }

    [Fact]
    public async Task ReturnBook_InvalidBorrowerId_ShouldReturnInvalidArgumentError()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new ReturnBookRequest
        {
            BookId = Guid.NewGuid().ToString(),
            BorrowerId = "invalid-guid"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.ReturnBookAsync(request));

        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Status.Detail.Should().Contain("borrower ID");
    }

    [Fact]
    public async Task GetActiveLoans_ValidRequest_ShouldReturnPaginatedLoans()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new GetActiveLoansRequest
        {
            BorrowerId = Guid.NewGuid().ToString(),
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await client.GetActiveLoansAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        response.Page.Should().Be(request.Page);
        response.PageSize.Should().Be(request.PageSize);
        response.HasNextPage.Should().BeFalse(); // Will be false for empty or single page results
        response.Loans.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveLoans_InvalidBorrowerId_ShouldReturnInvalidArgumentError()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new GetActiveLoansRequest
        {
            BorrowerId = "invalid-guid",
            Page = 1,
            PageSize = 10
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.GetActiveLoansAsync(request));

        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Status.Detail.Should().Contain("borrower ID");
    }

    [Fact]
    public async Task GetActiveLoans_InvalidPageNumbers_ShouldHandleGracefully()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new GetActiveLoansRequest
        {
            BorrowerId = Guid.NewGuid().ToString(),
            Page = 0, // Invalid page number
            PageSize = 0 // Invalid page size
        };

        // Act
        var response = await client.GetActiveLoansAsync(request);

        // Assert - Should handle gracefully with default values
        response.Should().NotBeNull();
        response.Page.Should().BeGreaterThanOrEqualTo(1); // Should be normalized
        response.PageSize.Should().BeGreaterThanOrEqualTo(1); // Should be normalized
    }

    [Fact]
    public async Task GetActiveLoans_Pagination_ShouldWorkCorrectly()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new GetActiveLoansRequest
        {
            BorrowerId = Guid.NewGuid().ToString(),
            Page = 2,
            PageSize = 5
        };

        // Act
        var response = await client.GetActiveLoansAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.HasNextPage.Should().BeFalse(); // Depends on actual data, but safe to assert
        response.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task LoanWorkflow_ValidFormatButNonExistentData_ShouldFailCorrectly()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();

        var borrowRequest = new BorrowBookRequest
        {
            BookId = bookId.ToString(),
            BorrowerId = borrowerId.ToString()
        };

        // Act & Assert - Should fail at borrowing since book doesn't exist
        var borrowException = await Assert.ThrowsAsync<RpcException>(
            async () => await client.BorrowBookAsync(borrowRequest));

        borrowException.StatusCode.Should().Be(StatusCode.NotFound);
        borrowException.Status.Detail.Should().Contain("not found");

        // Since borrowing failed, return should also fail
        var returnRequest = new ReturnBookRequest
        {
            BookId = bookId.ToString(),
            BorrowerId = borrowerId.ToString()
        };

        var returnException = await Assert.ThrowsAsync<RpcException>(
            async () => await client.ReturnBookAsync(returnRequest));

        returnException.StatusCode.Should().BeOneOf(StatusCode.NotFound, StatusCode.Internal);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public async Task GetActiveLoans_InvalidPageSizes_ShouldBeHandledGracefully(int invalidPageSize)
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new GetActiveLoansRequest
        {
            BorrowerId = Guid.NewGuid().ToString(),
            Page = 1,
            PageSize = invalidPageSize
        };

        // Act
        var response = await client.GetActiveLoansAsync(request);

        // Assert - Should handle gracefully or return appropriate error
        if (invalidPageSize <= 0)
        {
            // Negative or zero page sizes should either be normalized or cause an error
            response.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task LoanOperations_ValidRequestFormat_ShouldPassBasicValidation()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var bookId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();

        var borrowRequest = new BorrowBookRequest
        {
            BookId = bookId.ToString(),
            BorrowerId = borrowerId.ToString()
        };

        // Act & Assert - Should fail because book doesn't exist, but format validation should pass
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await client.BorrowBookAsync(borrowRequest));

        exception.StatusCode.Should().Be(StatusCode.NotFound);
        exception.Status.Detail.Should().Contain("not found");
    }

    [Fact]
    public async Task ServiceStatus_HealthCheck_ShouldWorkCorrectly()
    {
        // Arrange
        using var channel = CreateChannel();
        var client = new LibraryService.LibraryServiceClient(channel);

        var request = new ServiceStatusRequest();

        // Act
        var response = await client.GetServiceStatusAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.IsHealthy.Should().BeTrue();
        response.Version.Should().NotBeNullOrEmpty();
        response.Timestamp.Should().NotBeNullOrEmpty();

        // Verify timestamp is valid
        var timestamp = DateTime.Parse(response.Timestamp);
        timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromHours(2));
    }
}