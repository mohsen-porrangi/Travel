namespace UserManagement.API.Endpoints.Auth.RegisterUser;

public class VerifyRegisterOtpEndpointRemove : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register/verify-otp", async (
            VerifyRegisterOtpCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.NoContent();
        })
          .WithTags("Auth")
          .WithName("UserRegisterVerifyOtp")
          .WithDescription("تأیید کد یکبار مصرف برای فعال‌سازی حساب کاربری")
          .Produces(StatusCodes.Status204NoContent)
          .Produces(StatusCodes.Status401Unauthorized)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .AllowAnonymous();
    }
}
