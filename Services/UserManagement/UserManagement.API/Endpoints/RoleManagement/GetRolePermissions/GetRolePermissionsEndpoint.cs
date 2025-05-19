using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.RoleManagement.GetRolePermissions;

public class GetRolePermissionsEndpoint : ICarterModule
{
    [RequirePermission(RolePermissions.Read)]
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles/{id:guid}/permissions", async (
            int id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRolePermissionsQuery(id), ct);
            return Results.Ok(result);
        })
        .WithTags("Roles")
        .RequireAuthorization("Admin");
    }
}
