namespace UserManagement.API.Endpoints.Auth.Login;

public class LoginUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
            LoginUserCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var token = await sender.Send(command, ct);
            return Results.Ok(new { token });
        })
        .WithTags("Auth");
    }
}
