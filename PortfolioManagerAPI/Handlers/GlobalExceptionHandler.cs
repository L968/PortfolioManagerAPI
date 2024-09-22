using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PortfolioManagerAPI.Handlers;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        ProblemDetails problemDetails = CreateProblemDetails(exception);

        LogException(exception, problemDetails.Status!.Value, traceId);

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            AppException appException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = appException.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error"
            }
        };
    }

    private void LogException(Exception exception, int statusCode, string traceId)
    {
        string baseMessage = "Status Code: {StatusCode}, TraceId: {TraceId}, Message: {Message}";

        if (statusCode == StatusCodes.Status400BadRequest)
        {
            _logger.LogWarning(
                $"{baseMessage}, Validation warning occurred",
                statusCode,
                traceId,
                exception.Message
            );
        }
        else
        {
            _logger.LogError(
                exception,
                baseMessage + ", Error processing request on machine {MachineName}",
                statusCode,
                traceId,
                exception.Message,
                Environment.MachineName
            );
        }
    }
}
