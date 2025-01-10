using Admin.Application.Common.Interfaces;
using FastEndpoints;

namespace Admin.WebAPI.Endpoints.Users;

public class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUserEndpoint> _logger;

    public GetUserEndpoint(
        IUserService userService,
        ILogger<GetUserEndpoint> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/users/{Id}");
        Policies("AdminAccess");
        Description(d => d
            .WithTags("Users")
            .Produces<GetUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetUser")
            .WithOpenApi());
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        try
        {
            var user = await _userService.GetByIdAsync(req.Id, ct);
            if (user == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var permissions = await _userService.GetUserPermissionsAsync(user.Id, ct);

            var response = new GetUserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles.ToList(),
                Permissions = permissions.ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            await SendOkAsync(response, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}
