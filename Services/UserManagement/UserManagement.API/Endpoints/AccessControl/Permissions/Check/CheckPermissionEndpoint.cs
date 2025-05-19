using Carter;
using BuildingBlocks.Contracts.Security;

namespace UserManagement.API.Endpoints.AccessControl.Permissions.Check
{
    public class CheckPermissionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/permissions/check", async (
                CheckPermissionDTO request,
                IPermissionService permissionService) =>
            {
                var permissions = await permissionService.GetUserPermissionsAsync(request.UserId);

                return new CheckPermissionResponse(
                    IsGranted: permissions.Contains(request.Permission)
                );
            });
        }
    }
}
