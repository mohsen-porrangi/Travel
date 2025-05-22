using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Payment.Commands.ProcessPaymentCallback;
using WalletPayment.Application.Payment.Models;
using WalletPayment.Application.Payment.Services;

namespace WalletPayment.API.Endpoints.Payment;

public class PaymentCallbackEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // --- Endpoint یکپارچه‌شده برای کالبک پرداخت ---
        app.MapGet("/payments/callback", async (
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
        .WithTags("Payments")
        .AllowAnonymous();
    }
}