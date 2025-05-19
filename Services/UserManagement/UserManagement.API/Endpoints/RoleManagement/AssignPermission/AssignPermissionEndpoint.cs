using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.RoleManagement.AssignPermission;

public class AssignPermissionEndpoint : ICarterModule
{
    [RequirePermission(RolePermissions.AssignPermission)]

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/roles/{roleId:int}/permissions", async (
            int roleId,
            AssignPermissionCommand body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = body with { RoleId = roleId };
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("Roles")
        .RequireAuthorization("Admin");
    }
}
