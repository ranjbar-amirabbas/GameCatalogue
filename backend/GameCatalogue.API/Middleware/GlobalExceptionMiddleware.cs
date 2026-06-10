using System.Text.Json;
using GameCatalogue.Application.Exceptions;
using GameCatalogue.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = GameCatalogue.Application.Exceptions.ValidationException;

namespace GameCatalogue.API.Middleware;

/// <summary>
/// Middleware that converts unhandled exceptions into RFC 7807 ProblemDetails responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
    /// </summary>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the middleware.</summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case NotFoundException notFound:
                _logger.LogWarning(notFound, "Resource not found");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = notFound.Message,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
                };
                break;

            case ValidationException validation:
                _logger.LogWarning(validation, "Validation failed");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Detail = validation.Message,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
                };
                problem.Extensions["errors"] = validation.Errors;
                break;

            case DomainException domain:
                _logger.LogWarning(domain, "Domain rule violation");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "A domain rule was violated.",
                    Detail = domain.Message,
                    Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2"
                };
                break;

            default:
                _logger.LogError(exception, "An unhandled exception occurred");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = "An internal server error has occurred.",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
                };
                break;
        }

        problem.Instance = context.Request.Path;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status!.Value;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
