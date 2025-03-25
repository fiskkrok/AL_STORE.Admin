using FastEndpoints;

using System.Security.Claims;

namespace Admin.WebAPI.Endpoints.Diagnostics;

public class AuthDiagnosticEndpoint : EndpointWithoutRequest<object>
{
    public override void Configure()
    {
        Get("/diag/auth");
        Description(d => d
            .WithTags("Diagnostics")
            .Produces<object>(StatusCodes.Status200OK));
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var identity = HttpContext.User.Identity;

        var result = new
        {
            IsAuthenticated = identity?.IsAuthenticated ?? false,
            AuthType = identity?.AuthenticationType,
            Username = identity?.Name,

            Roles = HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList(),

            Scopes = HttpContext.User.Claims
                .Where(c => c.Type == "scope")
                .Select(c => c.Value)
                .ToList(),

            AllClaims = HttpContext.User.Claims
                .Select(c => new { Type = c.Type, Value = c.Value })
                .ToList(),

            HasJwtHeader = HttpContext.Request.Headers.ContainsKey("Authorization"),
            HasApiKeyHeader = HttpContext.Request.Headers.ContainsKey("X-API-Key")
        };

        return SendOkAsync(result, ct);
    }
}