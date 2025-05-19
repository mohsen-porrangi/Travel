namespace UserManagement.API.Endpoints.Auth.RegisterUser;

public class RegisterUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
            RegisterUserCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("Auth")
        .AllowAnonymous();
    }
}
