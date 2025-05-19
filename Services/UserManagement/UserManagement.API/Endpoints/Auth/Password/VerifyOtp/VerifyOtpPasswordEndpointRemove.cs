//namespace UserManagement.API.Endpoints.Auth.Password.VerifyOtp;

//public class VerifyResetPasswordOtpEndpoint : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapPost("/auth/password/verify-otp", async (
//            VerifyResetPasswordOtpCommand command,
//            ISender sender,
//            CancellationToken ct) =>
//        {
//            var isValid = await sender.Send(command, ct);
//            return Results.Ok(new { isValid });
//        })
//        .WithTags("Auth")
//        .AllowAnonymous();
//    }
//}