namespace UserManagement.API.Endpoints.Auth.Password.ResetPassword;

public class ResetPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/password/reset", async (
            ResetPasswordCommand command,
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