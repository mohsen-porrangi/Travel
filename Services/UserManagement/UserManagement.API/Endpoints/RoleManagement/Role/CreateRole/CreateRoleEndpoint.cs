using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.RoleManagement.Role.CreateRole;

public class CreateRoleEndpoint : ICarterModule
{
    [RequirePermission(RolePermissions.Create)]
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/roles", async (
            CreateRoleCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var id = await sender.Send(command, ct);
            return Results.Ok(new { id });
        })
        .WithTags("Roles")
        .RequireAuthorization("Admin");
    }
}
