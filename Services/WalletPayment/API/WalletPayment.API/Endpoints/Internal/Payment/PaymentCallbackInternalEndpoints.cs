using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Payment.Commands.ProcessPaymentCallback;
using WalletPayment.Application.Payment.Models;
using WalletPayment.Application.Payment.Services;

namespace WalletPayment.API.Endpoints.Internal.Payment;

public class PaymentCallbackInternalEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // --- Endpoint یکپارچه‌شده برای کالبک پرداخت ---
        app.MapGet("api/internal/payments/callback", async (
            HttpContext context,
            [FromQuery] string? integrated,
            [FromQuery] string? gatewayType,
            [FromServices]IPaymentCallbackProcessor callbackProcessor,
            ISender mediator,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // استخراج و پردازش پارامترهای کالبک
                var parameters = callbackProcessor.ExtractParameters(context.Request.Query, gatewayType);

                // پردازش کالبک
                var command = new ProcessPaymentCallbackCommand(parameters);
                var result = await mediator.Send(command, cancellationToken);

                // هدایت کاربر به مسیر مناسب
                return Results.Redirect(result.RedirectUrl);
            }
            catch (Exception)
            {
                // در صورت خطا، هدایت به صفحه خطا
                return Results.Redirect("/payment/failure?errorMessage=خطای سیستمی");
            }
        })
        .WithName("ProcessPaymentCallback")
.WithDescription("پردازش بازگشت از درگاه‌های پرداخت و تأیید تراکنش")
.Produces(StatusCodes.Status302Found) // Redirect
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError)
.WithTags("Payments")
        .AllowAnonymous();
    }
}