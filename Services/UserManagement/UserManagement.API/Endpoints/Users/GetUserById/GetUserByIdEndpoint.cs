using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.Users.GetUserById
{
    public class GetUserByIdEndpoint : ICarterModule
    {
        [RequirePermission(UserPermissions.View)]
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users/{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetUserByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("Users")
            .RequireAuthorization("Admin");
        }
    }
}
