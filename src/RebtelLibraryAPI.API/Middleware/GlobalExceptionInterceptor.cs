using Grpc.Core;
using Grpc.Core.Interceptors;

namespace RebtelLibraryAPI.API.Middleware;

/// <summary>
///     Global gRPC exception interceptor to handle uncaught exceptions and convert them to proper gRPC status codes
/// </summary>
public class GlobalExceptionInterceptor : Interceptor
{
    private readonly ILogger<GlobalExceptionInterceptor> _logger;

    public GlobalExceptionInterceptor(ILogger<GlobalExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            // Already a gRPC exception, let it propagate
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument validation failed for {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found for {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.NotFound, "Requested resource not found"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access for {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Authentication required"));
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Operation timed out for {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "Operation timed out"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred"));
        }
    }
}