using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Application.Transactions.Commands.IntegratedPurchase;

namespace WalletPayment.API.Endpoints.Payment;

public class PaymentCallbackEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/payments/callback", async (
            [FromQuery] string authority,
            [FromQuery] string status,
            [FromQuery] string userId,
            [FromQuery] decimal amount,
            [FromQuery] string orderId,
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<PaymentCallbackEndpoints> logger,
            CancellationToken cancellationToken) =>
        {
            logger.LogInformation(
                "دریافت بازگشت از درگاه پرداخت. وضعیت: {Status}, شناسه: {Authority}, کاربر: {UserId}",
                status, authority, userId);

            // بررسی و تبدیل پارامترها
            if (!Guid.TryParse(userId, out var userGuid))
            {
                logger.LogWarning("شناسه کاربر در بازگشت از درگاه نامعتبر است: {UserId}", userId);
                return Results.BadRequest(new { Message = "شناسه کاربر نامعتبر است" });
            }

            try
            {
                // تأیید پرداخت
                var result = await paymentService.VerifyPaymentAsync(
                    authority,
                    status,
                    amount,
                    userGuid,
                    true, // شارژ خودکار کیف پول
                    cancellationToken);

                // پیکربندی URL‌های بازگشت
                var successUrl = configuration["Payment:SuccessRedirectUrl"]
                    ?? "/payment/success";
                var failureUrl = configuration["Payment:FailureRedirectUrl"]
                    ?? "/payment/failure";

                // ساخت URL بازگشت با پارامترهای لازم
                var redirectUrl = new StringBuilder();

                if (result.IsSuccessful)
                {
                    redirectUrl.Append(successUrl);
                    redirectUrl.Append($"?orderId={Uri.EscapeDataString(orderId ?? "")}");
                    redirectUrl.Append($"&referenceId={Uri.EscapeDataString(result.ReferenceId ?? "")}");
                    redirectUrl.Append($"&amount={amount}");
                }
                else
                {
                    redirectUrl.Append(failureUrl);
                    redirectUrl.Append($"?orderId={Uri.EscapeDataString(orderId ?? "")}");
                    redirectUrl.Append($"&errorCode={result.ErrorCode?.ToString() ?? "Unknown"}");
                    redirectUrl.Append($"&errorMessage={Uri.EscapeDataString(result.ErrorMessage ?? "خطای نامشخص")}");
                }

                // هدایت کاربر
                return Results.Redirect(redirectUrl.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "خطا در پردازش بازگشت از درگاه پرداخت. شناسه: {Authority}", authority);
                return Results.Redirect(configuration["Payment:FailureRedirectUrl"]
                    ?? "/payment/failure?errorMessage=خطای سیستمی");
            }
        })
        .WithTags("Payments")
        .AllowAnonymous();

        app.MapGet("/payments/integrated-callback", async (
            [FromQuery] string authority,
            [FromQuery] string status,
            [FromQuery] string userId,
            [FromQuery] decimal amount,
            [FromQuery] string orderId,
            IPaymentService paymentService,
            ISender mediator,
            IConfiguration configuration,
            ILogger<PaymentCallbackEndpoints> logger,
            CancellationToken cancellationToken) =>
        {
            logger.LogInformation(
                "دریافت بازگشت از درگاه پرداخت یکپارچه. وضعیت: {Status}, شناسه: {Authority}, کاربر: {UserId}, سفارش: {OrderId}",
                status, authority, userId, orderId);

            // بررسی و تبدیل پارامترها
            if (!Guid.TryParse(userId, out var userGuid))
            {
                logger.LogWarning("شناسه کاربر در بازگشت از درگاه نامعتبر است: {UserId}", userId);
                return Results.BadRequest(new { Message = "شناسه کاربر نامعتبر است" });
            }

            try
            {
                // تأیید پرداخت
                var verifyResult = await paymentService.VerifyPaymentAsync(
                    authority,
                    status,
                    amount,
                    userGuid,
                    true, // شارژ خودکار کیف پول
                    cancellationToken);

                // پیکربندی URL‌های بازگشت
                var successUrl = configuration["Payment:IntegratedSuccessRedirectUrl"]
                    ?? "/payment/integrated/success";
                var failureUrl = configuration["Payment:IntegratedFailureRedirectUrl"]
                    ?? "/payment/integrated/failure";

                // ساخت URL بازگشت با پارامترهای لازم
                var redirectUrl = new StringBuilder();

                if (verifyResult.IsSuccessful)
                {
                    // انجام عملیات خرید یکپارچه
                    var integratedCommand = new IntegratedPurchaseCommand(
                        userGuid,
                        amount,
                        WalletPayment.Domain.Entities.Enums.CurrencyCode.IRR, // فرض می‌کنیم ریال است
                        orderId,
                        $"خرید با شناسه {orderId}",
                        verifyResult.ReferenceId
                    );

                    var purchaseResult = await mediator.Send(integratedCommand, cancellationToken);

                    redirectUrl.Append(successUrl);
                    redirectUrl.Append($"?orderId={Uri.EscapeDataString(orderId ?? "")}");
                    redirectUrl.Append($"&referenceId={Uri.EscapeDataString(verifyResult.ReferenceId ?? "")}");
                    redirectUrl.Append($"&amount={amount}");
                    redirectUrl.Append($"&transactionId={purchaseResult.WithdrawTransactionId}");
                }
                else
                {
                    redirectUrl.Append(failureUrl);
                    redirectUrl.Append($"?orderId={Uri.EscapeDataString(orderId ?? "")}");
                    redirectUrl.Append($"&errorCode={verifyResult.ErrorCode?.ToString() ?? "Unknown"}");
                    redirectUrl.Append($"&errorMessage={Uri.EscapeDataString(verifyResult.ErrorMessage ?? "خطای نامشخص")}");
                }

                // هدایت کاربر
                return Results.Redirect(redirectUrl.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "خطا در پردازش بازگشت از درگاه پرداخت یکپارچه. شناسه: {Authority}", authority);
                return Results.Redirect(configuration["Payment:IntegratedFailureRedirectUrl"]
                    ?? "/payment/integrated/failure?errorMessage=خطای سیستمی");
            }
        })
        .WithTags("Payments")
        .AllowAnonymous();

        // مسیرهای کالبک اختصاصی درگاه‌های مختلف

        // ZarinPal callback
        app.MapGet("/payments/callback/zarinpal", async (
            HttpContext context,
            IPaymentService paymentService,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var authority = context.Request.Query["Authority"].ToString();
            var status = context.Request.Query["Status"].ToString();
            var userId = context.Request.Query["userId"].ToString();
            decimal amount = 0;
            decimal.TryParse(context.Request.Query["Amount"].ToString(), out amount);
            var orderId = context.Request.Query["orderId"].ToString();

            // هدایت به مسیر عمومی callback
            var redirectUrl = $"/payments/callback?authority={Uri.EscapeDataString(authority)}" +
                             $"&status={Uri.EscapeDataString(status)}" +
                             $"&userId={Uri.EscapeDataString(userId)}" +
                             $"&amount={amount}" +
                             $"&orderId={Uri.EscapeDataString(orderId)}";

            return Results.Redirect(redirectUrl);
        })
        .WithTags("Payments")
        .AllowAnonymous();

        // Zibal callback
        app.MapGet("/payments/callback/zibal", async (
            HttpContext context,
            IPaymentService paymentService,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var trackId = context.Request.Query["trackId"].ToString();
            var success = context.Request.Query["success"].ToString();
            var status = success == "1" ? "OK" : "NOK";
            var userId = context.Request.Query["userId"].ToString();
            decimal amount = 0;
            decimal.TryParse(context.Request.Query["amount"].ToString(), out amount);
            var orderId = context.Request.Query["orderId"].ToString();

            // هدایت به مسیر عمومی callback
            var redirectUrl = $"/payments/callback?authority={Uri.EscapeDataString(trackId)}" +
                             $"&status={Uri.EscapeDataString(status)}" +
                             $"&userId={Uri.EscapeDataString(userId)}" +
                             $"&amount={amount}" +
                             $"&orderId={Uri.EscapeDataString(orderId)}";

            return Results.Redirect(redirectUrl);
        })
        .WithTags("Payments")
        .AllowAnonymous();

        // Sandbox callback
        app.MapGet("/payments/callback/sandbox", async (
            HttpContext context,
            IPaymentService paymentService,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var authority = context.Request.Query["authority"].ToString();
            var status = context.Request.Query["status"].ToString();
            var userId = context.Request.Query["userId"].ToString();
            decimal amount = 0;
            decimal.TryParse(context.Request.Query["amount"].ToString(), out amount);
            var orderId = context.Request.Query["orderId"].ToString();

            // هدایت به مسیر عمومی callback
            var redirectUrl = $"/payments/callback?authority={Uri.EscapeDataString(authority)}" +
                             $"&status={Uri.EscapeDataString(status)}" +
                             $"&userId={Uri.EscapeDataString(userId)}" +
                             $"&amount={amount}" +
                             $"&orderId={Uri.EscapeDataString(orderId)}";

            return Results.Redirect(redirectUrl);
        })
        .WithTags("Payments")
        .AllowAnonymous();
    }
}