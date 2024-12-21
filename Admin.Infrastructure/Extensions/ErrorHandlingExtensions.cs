using Admin.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Admin.Infrastructure.Extensions;

public static class ErrorHandlingExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
