using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HardenACrud.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = Map(exception);
        var isServerError = status >= StatusCodes.Status500InternalServerError;

        if (isServerError)
        {
            _logger.LogError(exception, "Unexpected error ({Status}) on {Method} {Path}",
                status, context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled {ExceptionType} ({Status}) on {Method} {Path}",
                exception.GetType().Name, status, context.Request.Method, context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = isServerError ? "Something went wrong. Please try again later." : exception.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }

    private static (int Status, string Title) Map(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
    };
}