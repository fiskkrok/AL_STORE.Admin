using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using Admin.Application.Common.Exceptions;
using Admin.Application.Common.Models;

using ValidationException = FluentValidation.ValidationException;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Global error handling middleware
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Code = "ValidationError";
                errorResponse.Message = "Validation failed";
                errorResponse.ValidationErrors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray());
                break;

            case NotFoundException notFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Code = "NotFound";
                errorResponse.Message = notFoundException.Message;
                break;

            case AppException appException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Code = appException.Code;
                errorResponse.Message = appException.Message;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Code = "Unauthorized";
                errorResponse.Message = "Unauthorized access";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Code = "InternalServerError";
                errorResponse.Message = "An internal server error occurred";
                if (_environment.IsDevelopment())
                {
                    errorResponse.StackTrace = exception.StackTrace;
                }
                break;
        }

        // Add request tracking information
        var requestId = context.TraceIdentifier;
        var endpoint = context.GetEndpoint()?.DisplayName;
        var user = context.User?.Identity?.Name ?? "anonymous";

        _logger.LogError(
            exception,
            "Error handling request. RequestId: {RequestId}, Endpoint: {Endpoint}, User: {User}, StatusCode: {StatusCode}, ErrorCode: {ErrorCode}",
            requestId,
            endpoint,
            user,
            response.StatusCode,
            errorResponse.Code);


        // Use a more controlled serialization approach
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        await context.Response.WriteAsJsonAsync(errorResponse, jsonOptions);
    }
}

/// <summary>
/// Extension method to add error handling middleware
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}