//using UserManagement.API.Endpoints.AccessControl.Permissions;

//namespace UserManagement.API.Endpoints.Users.DeactivateUser
//{
//    public class DeactivateUserEndpointRemove : ICarterModule
//    {
//        [RequirePermission(UserPermissions.Deactivate)]
//        public void AddRoutes(IEndpointRouteBuilder app)
//        {
//            app.MapPut("/users/{id:guid}/deactivate", async (
//                Guid id,
//                ISender sender,
//                CancellationToken ct) =>
//            {
//                await sender.Send(new DeactivateUserCommand(id), ct);
//                return Results.NoContent();
//            })
//            .WithTags("Users")
//            .RequireAuthorization("Admin");
//        }
//    }
//}