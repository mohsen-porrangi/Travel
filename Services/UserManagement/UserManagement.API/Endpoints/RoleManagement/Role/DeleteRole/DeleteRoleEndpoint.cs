using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.RoleManagement.Role.DeleteRole;

public class DeleteRoleEndpoint : ICarterModule
{
    [RequirePermission(RolePermissions.Delete)]
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/roles/{id:int}", async (
            int id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeleteRoleCommand(id), ct);
            return Results.NoContent();
        })
        .WithTags("Roles")
        .RequireAuthorization("Admin");
    }
}
