using System.Security.Claims;
using UserManagement.API.Common.Extensions;

namespace UserManagement.API.Endpoints.Auth.ChangePassword;

public class ChangePasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/change-password", async (
            ChangePasswordCommand command,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct) =>
        {
            var identityId = user.GetIdentityId();
            var finalCommand = command with { IdentityId = identityId };

            await sender.Send(finalCommand, ct);
            return Results.NoContent();
        })
        .WithTags("Auth")
        .RequireAuthorization();
    }
}
