namespace UserManagement.API.Endpoints.Auth.Password.ForgotPassword;

public class ForgotPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/password/forgot", async (
            ForgotPasswordCommand command,
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