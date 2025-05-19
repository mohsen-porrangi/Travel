using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.Users.ActivateUser
{
    public class ActivateUserEndpoint : ICarterModule
    {
        [RequirePermission(UserPermissions.Activate)]
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/users/{id:guid}/activate", async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                await sender.Send(new ActivateUserCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("Users")
            .RequireAuthorization("Admin");
        }
    }
}
