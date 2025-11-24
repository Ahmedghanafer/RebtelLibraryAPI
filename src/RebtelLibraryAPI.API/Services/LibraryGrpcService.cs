using Grpc.Core;
using MediatR;
using RebtelLibraryAPI.Application.Commands.Books;
using RebtelLibraryAPI.Application.Commands.Borrowers;
using RebtelLibraryAPI.Application.Commands.Loans;
using RebtelLibraryAPI.Application.Queries.Analytics;
using RebtelLibraryAPI.Application.Queries.Books;
using RebtelLibraryAPI.Application.Queries.Borrowers;
using RebtelLibraryAPI.Application.Queries.Loans;

namespace RebtelLibraryAPI.API.Services;

public class LibraryGrpcService : LibraryService.LibraryServiceBase
{
    private readonly ILogger<LibraryGrpcService> _logger;
    private readonly IMediator _mediator;

    public LibraryGrpcService(
        ILogger<LibraryGrpcService> logger,
        IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public override Task<ServiceStatusResponse> GetServiceStatus(
        ServiceStatusRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Service status requested");

        return Task.FromResult(new ServiceStatusResponse
        {
            IsHealthy = true,
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow.ToString("O")
        });
    }

    // Placeholder implementations for foundation story
    // These will be fully implemented in subsequent stories

    public override async Task<CreateBookResponse> CreateBook(
        CreateBookRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("CreateBook requested with Title: {Title}, ISBN: {ISBN}", request.Title, request.Isbn);

        try
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                _logger.LogWarning("CreateBook called with empty title");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Book title is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Isbn))
            {
                _logger.LogWarning("CreateBook called with empty ISBN");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ISBN is required"));
            }

            if (request.PageCount <= 0)
            {
                _logger.LogWarning("CreateBook called with invalid page count: {PageCount}", request.PageCount);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page count must be a positive number"));
            }

            var command = new CreateBookCommand(
                request.Title,
                request.Author,
                request.Isbn,
                request.PageCount,
                request.Category
            );

            var bookDto = await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Successfully created book with ID: {BookId}", bookDto.Id);

            return new CreateBookResponse
            {
                Id = bookDto.Id.ToString(),
                Message = "Book created successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book with ISBN: {ISBN}", request.Isbn);

            // Map known exception types to appropriate gRPC status codes
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            var statusCode = StatusCode.Internal;

            if (ex.Message.Contains("already exists") || ex.Message.Contains("DUPLICATE_ISBN") ||
                ex.Message.Contains("BORROWER_VALIDATION_ERROR"))
                statusCode = StatusCode.AlreadyExists;
            else if (ex.Message.Contains("required") || ex.Message.Contains("validation") ||
                     ex.Message.Contains("format"))
                statusCode = StatusCode.InvalidArgument;
            else if (ex.Message.Contains("not found")) statusCode = StatusCode.NotFound;

            throw new RpcException(new Status(statusCode, sanitizedMessage));
        }
    }

    public override async Task<UpdateBookResponse> UpdateBook(
        UpdateBookRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("UpdateBook requested with ID: {BookId}", request.Id);

        try
        {
            if (!Guid.TryParse(request.Id, out var bookId))
            {
                _logger.LogWarning("UpdateBook called with invalid BookId format: {BookId}", request.Id);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid book ID format"));
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                _logger.LogWarning("UpdateBook called with empty title");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Book title is required"));
            }

            if (request.PageCount <= 0)
            {
                _logger.LogWarning("UpdateBook called with invalid page count: {PageCount}", request.PageCount);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page count must be a positive number"));
            }

            var command = new UpdateBookCommand(
                bookId,
                request.Title,
                request.Author,
                request.PageCount,
                request.Category,
                request.IsAvailable
            );

            var bookDto = await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Successfully updated book with ID: {BookId}", bookDto.Id);

            return new UpdateBookResponse
            {
                Success = true,
                Message = "Book updated successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book with ID: {BookId}", request.Id);

            // Map known exception types to appropriate gRPC status codes
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            var statusCode = StatusCode.Internal;

            if (ex.Message.Contains("not found") || ex.Message.Contains("BOOK_NOT_FOUND"))
                statusCode = StatusCode.NotFound;
            else if (ex.Message.Contains("required") || ex.Message.Contains("validation"))
                statusCode = StatusCode.InvalidArgument;
            else if (ex.Message.Contains("already exists") || ex.Message.Contains("DUPLICATE_ISBN"))
                statusCode = StatusCode.AlreadyExists;

            throw new RpcException(new Status(statusCode, sanitizedMessage));
        }
    }

    public override async Task<GetBookResponse> GetBook(
        GetBookRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetBook requested with Id: {BookId}", request.Id);

        try
        {
            if (!Guid.TryParse(request.Id, out var bookId))
            {
                _logger.LogWarning("Invalid BookId format: {BookId}", request.Id);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid book ID format"));
            }

            var query = new GetBookQuery(bookId);
            var bookDto = await _mediator.Send(query, context.CancellationToken);

            if (bookDto == null)
            {
                _logger.LogInformation("Book with Id {BookId} not found", bookId);
                throw new RpcException(new Status(StatusCode.NotFound, $"Book with ID {bookId} not found"));
            }

            _logger.LogInformation("Successfully retrieved book with Id {BookId}", bookId);

            return new GetBookResponse
            {
                Book = MapToGrpcBookDto(bookDto)
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving book with Id {BookId}", request.Id);
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    public override async Task<ListBooksResponse> ListBooks(
        ListBooksRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "ListBooks requested with PageNumber: {PageNumber}, PageSize: {PageSize}, Category: {Category}",
            request.PageNumber, request.PageSize, request.CategoryFilter ?? "all");

        try
        {
            var pageSize = request.PageSize > 0 ? request.PageSize : 20;
            if (pageSize > 100) pageSize = 100;

            var query = new ListBooksQuery(
                request.PageNumber > 0 ? request.PageNumber : 1,
                pageSize,
                string.IsNullOrWhiteSpace(request.CategoryFilter) ? null : request.CategoryFilter,
                string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm);

            var listBooksDto = await _mediator.Send(query, context.CancellationToken);

            _logger.LogInformation(
                "Successfully retrieved {BookCount} books, page {PageNumber} of {TotalPages}",
                listBooksDto.Books.Count,
                listBooksDto.PageNumber,
                (int)Math.Ceiling((double)listBooksDto.TotalCount / listBooksDto.PageSize));

            var response = new ListBooksResponse
            {
                TotalCount = listBooksDto.TotalCount,
                PageNumber = listBooksDto.PageNumber,
                PageSize = listBooksDto.PageSize
            };

            foreach (var bookDto in listBooksDto.Books) response.Books.Add(MapToGrpcBookDto(bookDto));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books list");
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    public override async Task<RegisterBorrowerResponse> RegisterBorrower(
        RegisterBorrowerRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("RegisterBorrower requested with Name: {Name}, Email: {Email}", request.Name,
            request.Email);

        try
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("RegisterBorrower called with empty name");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Borrower name is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("RegisterBorrower called with empty email");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email is required"));
            }

            var command = new RegisterBorrowerCommand(
                request.Name,
                request.Email,
                request.Phone ?? string.Empty
            );

            var borrowerDto = await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Successfully registered borrower with ID: {BorrowerId}", borrowerDto.Id);

            return new RegisterBorrowerResponse
            {
                Id = borrowerDto.Id.ToString(),
                Message = "Borrower registered successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering borrower with email: {Email}", request.Email);

            // Map known exception types to appropriate gRPC status codes
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            var statusCode = StatusCode.Internal;

            if (ex.Message.Contains("already exists") || ex.Message.Contains("BORROWER_VALIDATION_ERROR"))
                statusCode = StatusCode.AlreadyExists;
            else if (ex.Message.Contains("required") || ex.Message.Contains("validation") ||
                     ex.Message.Contains("format"))
                statusCode = StatusCode.InvalidArgument;
            else if (ex.Message.Contains("not found")) statusCode = StatusCode.NotFound;

            throw new RpcException(new Status(statusCode, sanitizedMessage));
        }
    }

    public override async Task<UpdateBorrowerResponse> UpdateBorrower(
        UpdateBorrowerRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("UpdateBorrower requested with ID: {BorrowerId}", request.Id);

        try
        {
            if (!Guid.TryParse(request.Id, out var borrowerId))
            {
                _logger.LogWarning("UpdateBorrower called with invalid BorrowerId format: {BorrowerId}", request.Id);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid borrower ID format"));
            }

            // Create command with optional parameters
            var command = new UpdateBorrowerCommand(
                borrowerId,
                string.IsNullOrWhiteSpace(request.Name) ? null : request.Name,
                string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
                request.Phone,
                request.IsActive || !request.IsActive ? request.IsActive : null
            );

            var borrowerDto = await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Successfully updated borrower with ID: {BorrowerId}", borrowerDto.Id);

            return new UpdateBorrowerResponse
            {
                Success = true,
                Message = "Borrower updated successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating borrower with ID: {BorrowerId}", request.Id);

            // Map known exception types to appropriate gRPC status codes
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            var statusCode = StatusCode.Internal;

            if (ex.Message.Contains("not found") || ex.Message.Contains("BORROWER_NOT_FOUND"))
                statusCode = StatusCode.NotFound;
            else if (ex.Message.Contains("required") || ex.Message.Contains("validation") ||
                     ex.Message.Contains("format"))
                statusCode = StatusCode.InvalidArgument;
            else if (ex.Message.Contains("already exists") || ex.Message.Contains("already in use"))
                statusCode = StatusCode.AlreadyExists;

            throw new RpcException(new Status(statusCode, sanitizedMessage));
        }
    }

    public override async Task<GetBorrowerResponse> GetBorrower(
        GetBorrowerRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetBorrower requested with Id: {BorrowerId}", request.Id);

        try
        {
            if (!Guid.TryParse(request.Id, out var borrowerId))
            {
                _logger.LogWarning("Invalid BorrowerId format: {BorrowerId}", request.Id);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid borrower ID format"));
            }

            var query = new GetBorrowerQuery(borrowerId);
            var borrowerDto = await _mediator.Send(query, context.CancellationToken);

            if (borrowerDto == null)
            {
                _logger.LogInformation("Borrower with Id {BorrowerId} not found", borrowerId);
                throw new RpcException(new Status(StatusCode.NotFound, $"Borrower with ID {borrowerId} not found"));
            }

            _logger.LogInformation("Successfully retrieved borrower with Id {BorrowerId}", borrowerId);

            return new GetBorrowerResponse
            {
                Borrower = MapToGrpcBorrowerDto(borrowerDto)
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrower with Id {BorrowerId}", request.Id);
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    public override async Task<ListBorrowersResponse> ListBorrowers(
        ListBorrowersRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "ListBorrowers requested with PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
            request.PageNumber, request.PageSize, request.SearchTerm ?? "none");

        try
        {
            var pageSize = request.PageSize > 0 ? request.PageSize : 20;
            if (pageSize > 100) pageSize = 100;

            var query = new ListBorrowersQuery(
                request.PageNumber > 0 ? request.PageNumber : 1,
                pageSize,
                string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm,
                string.IsNullOrWhiteSpace(request.MemberStatusFilter) ? null : request.MemberStatusFilter
            );

            var listBorrowersDto = await _mediator.Send(query, context.CancellationToken);

            _logger.LogInformation(
                "Successfully retrieved {BorrowerCount} borrowers, page {PageNumber} of {TotalPages}",
                listBorrowersDto.Borrowers.Count,
                listBorrowersDto.PageNumber,
                listBorrowersDto.TotalPages);

            var response = new ListBorrowersResponse
            {
                TotalCount = listBorrowersDto.TotalCount,
                PageNumber = listBorrowersDto.PageNumber,
                PageSize = listBorrowersDto.PageSize
            };

            foreach (var borrowerDto in listBorrowersDto.Borrowers)
                response.Borrowers.Add(MapToGrpcBorrowerDto(borrowerDto));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrowers list");
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    // Loan Management Operations

    public override async Task<BorrowBookResponse> BorrowBook(
        BorrowBookRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("BorrowBook requested for BookId: {BookId}, BorrowerId: {BorrowerId}",
            request.BookId, request.BorrowerId);

        try
        {
            if (!Guid.TryParse(request.BookId, out var bookId))
            {
                _logger.LogWarning("BorrowBook called with invalid BookId format: {BookId}", request.BookId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid book ID format"));
            }

            if (!Guid.TryParse(request.BorrowerId, out var borrowerId))
            {
                _logger.LogWarning("BorrowBook called with invalid BorrowerId format: {BorrowerId}",
                    request.BorrowerId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid borrower ID format"));
            }

            var command = new BorrowBookCommand(bookId, borrowerId);
            var loanDto = await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Successfully borrowed book {BookId} by borrower {BorrowerId}, LoanId: {LoanId}",
                bookId, borrowerId, loanDto.Id);

            return new BorrowBookResponse
            {
                Loan = MapToGrpcLoanDto(loanDto),
                Message = "Book borrowed successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error borrowing book {BookId} by borrower {BorrowerId}",
                request.BookId, request.BorrowerId);

            // Map known exception types to appropriate gRPC status codes
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            var statusCode = StatusCode.Internal;

            if (ex.Message.Contains("not found") || ex.Message.Contains("BOOK_NOT_FOUND") ||
                ex.Message.Contains("BORROWER_NOT_FOUND"))
                statusCode = StatusCode.NotFound;
            else if (ex.Message.Contains("not available") || ex.Message.Contains("BOOK_NOT_AVAILABLE") ||
                     ex.Message.Contains("BORROWER_NOT_ACTIVE"))
                statusCode = StatusCode.FailedPrecondition;
            else if (ex.Message.Contains("required") || ex.Message.Contains("validation"))
                statusCode = StatusCode.InvalidArgument;
            else if (ex.Message.Contains("already exists") || ex.Message.Contains("already borrowed"))
                statusCode = StatusCode.AlreadyExists;

            throw new RpcException(new Status(statusCode, sanitizedMessage));
        }
    }

    public override async Task<ReturnBookResponse> ReturnBook(
        ReturnBookRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("ReturnBook requested for BookId: {BookId}, BorrowerId: {BorrowerId}",
            request.BookId, request.BorrowerId);

        try
        {
            if (!Guid.TryParse(request.BookId, out var bookId))
            {
                _logger.LogWarning("ReturnBook called with invalid BookId format: {BookId}", request.BookId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid book ID format"));
            }

            if (!Guid.TryParse(request.BorrowerId, out var borrowerId))
            {
                _logger.LogWarning("ReturnBook called with invalid BorrowerId format: {BorrowerId}",
                    request.BorrowerId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid borrower ID format"));
            }

            var command = new ReturnBookCommand(bookId, borrowerId);
            var loanDto = await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("Successfully returned book {BookId} by borrower {BorrowerId}, LoanId: {LoanId}",
                bookId, borrowerId, loanDto.Id);

            return new ReturnBookResponse
            {
                Loan = MapToGrpcLoanDto(loanDto),
                Message = "Book returned successfully"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning book {BookId} by borrower {BorrowerId}",
                request.BookId, request.BorrowerId);

            // Map known exception types to appropriate gRPC status codes
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            var statusCode = StatusCode.Internal;

            if (ex.Message.Contains("not found") || ex.Message.Contains("BOOK_NOT_FOUND") ||
                ex.Message.Contains("LOAN_NOT_FOUND"))
                statusCode = StatusCode.NotFound;
            else if (ex.Message.Contains("required") || ex.Message.Contains("validation"))
                statusCode = StatusCode.InvalidArgument;
            else if (ex.Message.Contains("operation") || ex.Message.Contains("LOAN_OPERATION_ERROR"))
                statusCode = StatusCode.FailedPrecondition;

            throw new RpcException(new Status(statusCode, sanitizedMessage));
        }
    }

    public override async Task<GetActiveLoansResponse> GetActiveLoans(
        GetActiveLoansRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "GetActiveLoans requested for BorrowerId: {BorrowerId}, Page: {Page}, PageSize: {PageSize}",
            request.BorrowerId, request.Page, request.PageSize);

        try
        {
            if (!Guid.TryParse(request.BorrowerId, out var borrowerId))
            {
                _logger.LogWarning("GetActiveLoans called with invalid BorrowerId format: {BorrowerId}",
                    request.BorrowerId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid borrower ID format"));
            }

            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            if (pageSize > 100) pageSize = 100;

            var query = new GetActiveLoansQuery(borrowerId, page, pageSize);
            var listLoansResponse = await _mediator.Send(query, context.CancellationToken);

            _logger.LogInformation("Successfully retrieved {LoanCount} active loans for borrower {BorrowerId}",
                listLoansResponse.Loans.Count, borrowerId);

            var response = new GetActiveLoansResponse
            {
                TotalCount = listLoansResponse.TotalCount,
                Page = listLoansResponse.Page,
                PageSize = listLoansResponse.PageSize,
                HasNextPage = listLoansResponse.HasNextPage
            };

            foreach (var loanDto in listLoansResponse.Loans) response.Loans.Add(MapToGrpcLoanDto(loanDto));

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active loans for borrower {BorrowerId}", request.BorrowerId);

            // Map known exception types to appropriate gRPC status codes
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            var statusCode = StatusCode.Internal;

            if (ex.Message.Contains("required") || ex.Message.Contains("validation"))
                statusCode = StatusCode.InvalidArgument;

            throw new RpcException(new Status(statusCode, sanitizedMessage));
        }
    }

    // Analytics Operations

    public override async Task<GetMostBorrowedBooksResponse> GetMostBorrowedBooks(
        GetMostBorrowedBooksRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "GetMostBorrowedBooks requested from {StartDate} to {EndDate}, Page: {Page}, PageSize: {PageSize}",
            request.StartDate, request.EndDate, request.Page, request.PageSize);

        try
        {
            if (!DateTime.TryParse(request.StartDate, out var startDate))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid start date format"));

            if (!DateTime.TryParse(request.EndDate, out var endDate))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid end date format"));

            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            if (pageSize > 100) pageSize = 100;

            var query = new GetMostBorrowedBooksQuery(startDate, endDate, page, pageSize);
            var analyticsResponse = await _mediator.Send(query, context.CancellationToken);

            var response = new GetMostBorrowedBooksResponse
            {
                TotalCount = analyticsResponse.TotalCount,
                Page = analyticsResponse.Page,
                PageSize = analyticsResponse.PageSize,
                HasNextPage = analyticsResponse.HasNextPage
            };

            foreach (var bookAnalytics in analyticsResponse.Books)
                response.Books.Add(MapToGrpcBookAnalyticsDto(bookAnalytics));

            _logger.LogInformation("Successfully retrieved {BookCount} most borrowed books", response.Books.Count);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving most borrowed books");
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    public override async Task<GetMostActiveBorrowersResponse> GetMostActiveBorrowers(
        GetMostActiveBorrowersRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "GetMostActiveBorrowers requested from {StartDate} to {EndDate}, Page: {Page}, PageSize: {PageSize}",
            request.StartDate, request.EndDate, request.Page, request.PageSize);

        try
        {
            if (!DateTime.TryParse(request.StartDate, out var startDate))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid start date format"));

            if (!DateTime.TryParse(request.EndDate, out var endDate))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid end date format"));

            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            if (pageSize > 100) pageSize = 100;

            var query = new GetMostActiveBorrowersQuery(startDate, endDate, page, pageSize);
            var analyticsResponse = await _mediator.Send(query, context.CancellationToken);

            var response = new GetMostActiveBorrowersResponse
            {
                TotalCount = analyticsResponse.TotalCount,
                Page = analyticsResponse.Page,
                PageSize = analyticsResponse.PageSize,
                HasNextPage = analyticsResponse.HasNextPage
            };

            foreach (var borrowerAnalytics in analyticsResponse.Borrowers)
                response.Borrowers.Add(MapToGrpcBorrowerAnalyticsDto(borrowerAnalytics));

            _logger.LogInformation("Successfully retrieved {BorrowerCount} most active borrowers",
                response.Borrowers.Count);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving most active borrowers");
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    public override async Task<EstimateReadingPaceResponse> EstimateReadingPace(
        EstimateReadingPaceRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("EstimateReadingPace requested for BorrowerId: {BorrowerId}", request.BorrowerId);

        try
        {
            if (!Guid.TryParse(request.BorrowerId, out var borrowerId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid borrower ID format"));

            var query = new EstimateReadingPaceQuery(borrowerId);
            var readingPaceResponse = await _mediator.Send(query, context.CancellationToken);

            var response = new EstimateReadingPaceResponse
            {
                BorrowerId = readingPaceResponse.BorrowerId.ToString(),
                AveragePagesPerDay = (double)readingPaceResponse.AveragePagesPerDay,
                LoanCountUsed = readingPaceResponse.LoanCountUsed,
                HasSufficientData = readingPaceResponse.HasSufficientData,
                Message = readingPaceResponse.Message ?? string.Empty
            };

            _logger.LogInformation(
                "Successfully calculated reading pace for borrower {BorrowerId}: {PagesPerDay:F2} pages/day",
                borrowerId, response.AveragePagesPerDay);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating reading pace for borrower {BorrowerId}", request.BorrowerId);
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    public override async Task<GetBookRecommendationsResponse> GetBookRecommendations(
        GetBookRecommendationsRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("GetBookRecommendations requested for BookId: {BookId}, Limit: {Limit}",
            request.BookId, request.Limit);

        try
        {
            if (!Guid.TryParse(request.BookId, out var bookId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid book ID format"));

            var limit = request.Limit > 0 ? request.Limit : 5;
            if (limit > 50) limit = 50;

            var query = new GetBookRecommendationsQuery(bookId, limit);
            var analyticsResponse = await _mediator.Send(query, context.CancellationToken);

            var response = new GetBookRecommendationsResponse
            {
                TotalCount = analyticsResponse.TotalCount,
                Page = analyticsResponse.Page,
                PageSize = analyticsResponse.PageSize,
                HasNextPage = analyticsResponse.HasNextPage
            };

            foreach (var bookAnalytics in analyticsResponse.Books)
                response.Books.Add(MapToGrpcBookAnalyticsDto(bookAnalytics));

            _logger.LogInformation("Successfully retrieved {BookCount} book recommendations", response.Books.Count);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book recommendations for book {BookId}", request.BookId);
            var sanitizedMessage = ErrorMessageSanitizer.Sanitize(ex);
            throw new RpcException(new Status(StatusCode.Internal, sanitizedMessage));
        }
    }

    // Helper methods for mapping between DTOs and gRPC messages
    private static BookDto MapToGrpcBookDto(Application.DTOs.BookDto bookDto)
    {
        return new BookDto
        {
            Id = bookDto.Id.ToString(),
            Title = bookDto.Title,
            Author = bookDto.Author,
            Isbn = bookDto.ISBN,
            PageCount = bookDto.PageCount,
            Category = bookDto.Category,
            IsAvailable = bookDto.Availability == "Available",
            CreatedAt = bookDto.CreatedAt.ToString("O"),
            UpdatedAt = bookDto.UpdatedAt?.ToString("O")
        };
    }

    private static BorrowerDto MapToGrpcBorrowerDto(Application.DTOs.BorrowerDto borrowerDto)
    {
        return new BorrowerDto
        {
            Id = borrowerDto.Id.ToString(),
            FirstName = borrowerDto.FirstName,
            LastName = borrowerDto.LastName,
            Email = borrowerDto.Email,
            PhoneNumber = borrowerDto.Phone ?? string.Empty,
            RegistrationDate = borrowerDto.RegistrationDate.ToString("O"),
            IsActive = borrowerDto.MemberStatus == "Active",
            ActiveLoansCount = 0 // TODO: Implement loan count when loan system is ready
        };
    }

    private static LoanDto MapToGrpcLoanDto(Application.DTOs.LoanDto loanDto)
    {
        return new LoanDto
        {
            Id = loanDto.Id.ToString(),
            BookId = loanDto.BookId.ToString(),
            BorrowerId = loanDto.BorrowerId.ToString(),
            BorrowDate = loanDto.BorrowDate.ToString("O"),
            DueDate = loanDto.DueDate.ToString("O"),
            ReturnDate = loanDto.ReturnDate?.ToString("O"),
            Status = loanDto.Status,
            IsOverdue = loanDto.IsOverdue,
            DaysOverdue = loanDto.DaysOverdue,
            OverdueFee = (double)loanDto.OverdueFee,
            CreatedAt = loanDto.CreatedAt.ToString("O"),
            UpdatedAt = loanDto.UpdatedAt?.ToString("O")
        };
    }

    private static BookAnalyticsDto MapToGrpcBookAnalyticsDto(Application.DTOs.Analytics.BookAnalyticsDto bookAnalytics)
    {
        return new BookAnalyticsDto
        {
            Id = bookAnalytics.Id.ToString(),
            Title = bookAnalytics.Title,
            Author = bookAnalytics.Author,
            Isbn = bookAnalytics.ISBN,
            BorrowCount = bookAnalytics.BorrowCount,
            PageCount = bookAnalytics.PageCount,
            Category = bookAnalytics.Category
        };
    }

    private static BorrowerAnalyticsDto MapToGrpcBorrowerAnalyticsDto(
        Application.DTOs.Analytics.BorrowerAnalyticsDto borrowerAnalytics)
    {
        return new BorrowerAnalyticsDto
        {
            Id = borrowerAnalytics.Id.ToString(),
            BorrowCount = borrowerAnalytics.BorrowCount
        };
    }
}