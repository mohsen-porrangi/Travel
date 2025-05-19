using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.RoleManagement.Role.UpdateRole;

public class UpdateRoleEndpoint : ICarterModule
{
    [RequirePermission(RolePermissions.Create)]
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/roles/{id:int}", async (
            int id,
            UpdateRoleCommand body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = body with { Id = id };
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("Roles")
        .RequireAuthorization("Admin");
    }
}
