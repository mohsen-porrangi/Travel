//using UserManagement.API.Endpoints.AccessControl.Permissions;

//namespace UserManagement.API.Endpoints.RoleManagement.UnassignPermission;

//public class UnassignPermissionEndpointRemove : ICarterModule
//{
//    [RequirePermission(RolePermissions.AssignPermission)]
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapDelete("/roles/{roleId:int}/permissions/{permissionId:int}", async (
//            int roleId,
//            int permissionId,
//            ISender sender,
//            CancellationToken ct) =>
//        {
//            var command = new UnassignPermissionCommand(roleId, permissionId);
//            await sender.Send(command, ct);
//            return Results.NoContent();
//        })
//        .WithTags("Roles")
//        .RequireAuthorization("Admin");
//    }
//}
