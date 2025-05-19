using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.RoleManagement.Role.GetAllRoles;

public class GetAllRolesEndpoint : ICarterModule
{
    [RequirePermission(RolePermissions.Read)]
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllRolesQuery(), ct);
            return Results.Ok(result);
        })
        .WithTags("Roles")
        .RequireAuthorization("Admin");
    }
}
