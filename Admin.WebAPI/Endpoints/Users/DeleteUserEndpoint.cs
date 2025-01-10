using Admin.Application.Common.Interfaces;
using FastEndpoints;

namespace Admin.WebAPI.Endpoints.Users;

public class DeleteUserEndpoint : Endpoint<DeleteUserRequest>
{
    private readonly IUserService _userService;
    private readonly ILogger<DeleteUserEndpoint> _logger;

    public DeleteUserEndpoint(
        IUserService userService,
        ILogger<DeleteUserEndpoint> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("/users/{Id}");
        Policies("AdminAccess");
        Description(d => d
            .WithTags("Users")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteUser")
            .WithOpenApi());
    }

    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        try
        {
            await _userService.DeleteUserAsync(req.Id, ct);
            await SendNoContentAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}
