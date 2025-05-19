using System.Security.Claims;
using UserManagement.API.Common.Extensions;

namespace UserManagement.API.Endpoints.Profile.GetCurrentUser
{
    public class GetCurrentUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users/current", async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken ct) =>
            {
                var identityId = user.GetIdentityId(); 
                var result = await sender.Send(new GetCurrentUserQuery(identityId), ct);
                return Results.Ok(result);
            })
            .WithTags("Profile")
            .RequireAuthorization();
        }
    }
}
