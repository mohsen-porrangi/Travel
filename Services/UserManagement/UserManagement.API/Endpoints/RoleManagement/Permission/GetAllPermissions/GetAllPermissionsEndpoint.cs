using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.RoleManagement.Permission.GetAllPermissions;

public class GetAllPermissionsEndpoint : ICarterModule
{
    [RequirePermission(RolePermissions.Read)]
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/permissions", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllPermissionsQuery(), ct);
            return Results.Ok(result);
        })
        .WithTags("Permissions")
        .RequireAuthorization("Admin");
    }
}
