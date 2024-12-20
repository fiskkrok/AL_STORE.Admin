//using System.Text.Json;
//using Admin.Application.Common.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;

//namespace Admin.Infrastructure.Middleware;
//public class ExceptionHandlingMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

//    public ExceptionHandlingMiddleware(
//        RequestDelegate next,
//        ILogger<ExceptionHandlingMiddleware> logger)
//    {
//        _next = next;
//        _logger = logger;
//    }


//    public async Task InvokeAsync(HttpContext context)
//    {
//        try
//        {
//            await _next(context);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An unhandled exception has occurred");
//            await HandleExceptionAsync(context, ex);
//        }
//    }

//    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
//    {
//        var (statusCode, errorResponse) = exception switch
//        {
//            Domain.Common.Exceptions.DomainException e =>
//                (StatusCodes.Status400BadRequest,
//                new ErrorResponse("Domain Error", e.Message)),

//            Application.Common.Exceptions.ValidationException e =>
//                (StatusCodes.Status400BadRequest,
//                new ErrorResponse("Validation Error",
//                    e.Errors.SelectMany(x => x.Value).ToArray())),

//            Application.Common.Exceptions.NotFoundException e =>
//                (StatusCodes.Status404NotFound,
//                new ErrorResponse("Not Found", e.Message)),

//            Common.Exceptions.StorageException =>
//                (StatusCodes.Status503ServiceUnavailable,
//                new ErrorResponse("Storage Error", "A storage operation failed")),

//            _ => (StatusCodes.Status500InternalServerError,
//                new ErrorResponse("Server Error", "An unexpected error occurred"))
//        };

//        context.Response.ContentType = "application/json";
//        context.Response.StatusCode = statusCode;

//        var jsonResponse = JsonSerializer.Serialize(errorResponse);
//        await context.Response.WriteAsync(jsonResponse);
//    }
//}

