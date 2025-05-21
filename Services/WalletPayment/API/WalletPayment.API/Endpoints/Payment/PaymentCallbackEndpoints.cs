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
        // --- ثبت یک endpoint عمومی کالبک با پارامتر تشخیص درگاه ---
        app.MapGet("/payments/callback/{gatewayType?}", async (
            [FromRoute] string? gatewayType,
            HttpContext context,
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<PaymentCallbackEndpoints> logger,
            CancellationToken cancellationToken) =>
        {
            // دریافت پارامترهای کالبک بر اساس نوع درگاه
            string authority;
            string status;
            string userId;
            decimal amount = 0;
            string orderId = "";

            // تشخیص نوع درگاه یا استفاده از پارامتر ورودی
            var gateway = gatewayType?.ToLower() ?? "";

            // پردازش پارامترها بر اساس نوع درگاه
            if (gateway == "zarinpal" || context.Request.Query.ContainsKey("Authority"))
            {
                // ZarinPal format
                authority = context.Request.Query["Authority"].ToString();
                status = context.Request.Query["Status"].ToString();
                userId = context.Request.Query["userId"].ToString();
                decimal.TryParse(context.Request.Query["Amount"].ToString(), out amount);
                orderId = context.Request.Query["orderId"].ToString();
            }
            else if (gateway == "zibal" || context.Request.Query.ContainsKey("trackId"))
            {
                // Zibal format
                authority = context.Request.Query["trackId"].ToString();
                status = context.Request.Query["success"].ToString() == "1" ? "OK" : "NOK";
                userId = context.Request.Query["userId"].ToString();
                decimal.TryParse(context.Request.Query["amount"].ToString(), out amount);
                orderId = context.Request.Query["orderId"].ToString();
            }
            else if (gateway == "sandbox" || context.Request.Query.ContainsKey("authority"))
            {
                // Sandbox format (lowercase 'authority')
                authority = context.Request.Query["authority"].ToString();
                status = context.Request.Query["status"].ToString();
                userId = context.Request.Query["userId"].ToString();
                decimal.TryParse(context.Request.Query["amount"].ToString(), out amount);
                orderId = context.Request.Query["orderId"].ToString();
            }
            else
            {
                // دیگر درگاه‌ها یا فرمت ناشناخته
                logger.LogWarning("فرمت کالبک ناشناخته: {QueryString}", context.Request.QueryString);
                return Results.BadRequest(new { Message = "پارامترهای کالبک نامعتبر است" });
            }

            logger.LogInformation(
                "دریافت بازگشت از درگاه پرداخت. وضعیت: {Status}, شناسه: {Authority}, کاربر: {UserId}, شناسه سفارش: {OrderId}",
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

        // Endpoint کالبک برای خرید یکپارچه به همان شکل حفظ می‌شود
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
    
    }
}