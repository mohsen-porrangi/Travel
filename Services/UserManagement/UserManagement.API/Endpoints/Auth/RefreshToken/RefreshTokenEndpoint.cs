using UserManagement.API.Endpoints.Auth.RefreshToken;

public class RefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh-token", async (
            RefreshTokenCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Ok(new
            {
                token = result.AccessToken,
                refreshToken = result.RefreshToken
            });
        })
        .WithTags("Auth")
        .AllowAnonymous();
    }
}
