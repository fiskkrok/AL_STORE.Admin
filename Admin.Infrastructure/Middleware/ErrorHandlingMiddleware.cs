using Admin.Application.Common.Exceptions;
using Admin.Application.Common.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ValidationException = FluentValidation.ValidationException;

namespace Admin.Infrastructure.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

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
        var (statusCode, response) = MapExceptionToResponse(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        // Add stack trace in development
        if (_env.IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
        }

        // Log the error with structured data
        _logger.LogError(
            exception,
            "Error processing request: {ErrorCode} - {ErrorMessage} {@ValidationErrors}",
            response.Code,
            response.Message,
            response.ValidationErrors);

        await context.Response.WriteAsJsonAsync(response);
    }

    private static (int statusCode, ErrorResponse response) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Code = "Validation.Error",
                    Message = "One or more validation errors occurred",
                    ValidationErrors = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray())
                }),

            EntityNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ErrorResponse
                {
                    Code = notFoundEx.Code,
                    Message = notFoundEx.Message
                }),

            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                new ErrorResponse
                {
                    Code = conflictEx.Code,
                    Message = conflictEx.Message
                }),

            DomainRuleException domainEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Code = domainEx.Code,
                    Message = domainEx.Message
                }),

            AppException appEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Code = appEx.Code,
                    Message = appEx.Message
                }),

            // Default case for unexpected exceptions
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "Internal.Error",
                    Message = "An unexpected error occurred"
                })
        };
    }
}
