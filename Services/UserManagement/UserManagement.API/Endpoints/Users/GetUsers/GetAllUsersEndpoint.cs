using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.Users.GetUsers
{
    public class GetAllUsersEndpoint : ICarterModule
    {
       // [RequirePermission(UserPermissions.Read)]
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAllUsersQuery(), ct);
                return Results.Ok(result);
            })
             .WithName("ListAllUsers")
.WithDescription("دریافت لیست تمام کاربران")
.Produces<GetAllUsersResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
                .WithTags("Users")
                .RequireAuthorization("Admin");
        }
    }
}