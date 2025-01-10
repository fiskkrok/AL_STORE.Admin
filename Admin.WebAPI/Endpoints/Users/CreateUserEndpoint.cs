using Admin.Application.Common.Interfaces;
using Admin.Application.Services;
using FastEndpoints;

namespace Admin.WebAPI.Endpoints.Users;

public class CreateUserEndpoint : Endpoint<CreateUserRequest, Guid>
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILogger<CreateUserEndpoint> _logger;

    public CreateUserEndpoint(
        IUserService userService,
        IRoleService roleService,
        ILogger<CreateUserEndpoint> logger)
    {
        _userService = userService;
        _roleService = roleService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users");
        Policies("AdminAccess");
        Description(d => d
            .WithTags("Users")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateUser")
            .WithOpenApi());
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        try
        {
            // Validate roles
            if (req.Roles.Any(role => !_roleService.IsValidRole(role)))
            {
                await SendErrorsAsync(400, ct);
                return;
            }

            var user = await _userService.CreateUserAsync(
                req.Username,
                req.Email,
                req.Password,
                req.Roles,
                ct);

            await SendCreatedAtAsync<GetUserEndpoint>(
                new { id = user.Id },
                user.Id,
                generateAbsoluteUrl: true,
                cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            await SendErrorsAsync(500, ct);
        }
    }
}
