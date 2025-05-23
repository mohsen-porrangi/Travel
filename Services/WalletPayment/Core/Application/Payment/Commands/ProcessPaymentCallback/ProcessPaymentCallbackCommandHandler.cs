using BuildingBlocks.CQRS;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using WalletPayment.Application.Common.Exceptions;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Application.Transactions.Commands.ProcessTransaction;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Payment.Commands.ProcessPaymentCallback;

public class ProcessPaymentCallbackCommandHandler(
    IPaymentService paymentService,
    ISender mediator,
    IConfiguration configuration,
    ILogger<ProcessPaymentCallbackCommandHandler> logger
    ) : ICommandHandler<ProcessPaymentCallbackCommand, PaymentCallbackResult>
{
    public async Task<PaymentCallbackResult> Handle(
        ProcessPaymentCallbackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var parameters = request.Parameters;

            // تعیین آدرس‌های بازگشت بر اساس نوع پرداخت
            string successUrl, failureUrl;

            if (parameters.IsIntegrated)
            {
                successUrl = configuration["Payment:IntegratedSuccessRedirectUrl"] ?? "/payment/integrated/success";
                failureUrl = configuration["Payment:IntegratedFailureRedirectUrl"] ?? "/payment/integrated/failure";
            }
            else
            {
                successUrl = configuration["Payment:SuccessRedirectUrl"] ?? "/payment/success";
                failureUrl = configuration["Payment:FailureRedirectUrl"] ?? "/payment/failure";
            }

            // تأیید پرداخت
            var verifyResult = await paymentService.VerifyPaymentAsync(
                parameters.Authority,
                parameters.Status,
                parameters.Amount,
                parameters.UserId,
                true, // شارژ خودکار کیف پول
                cancellationToken);

            // ساخت URL بازگشت
            var redirectUrl = new StringBuilder();
            var result = new PaymentCallbackResult();

            if (verifyResult.IsSuccessful)
            {
                // پرداخت موفق
                result.IsSuccessful = true;
                result.ReferenceId = verifyResult.ReferenceId;

                // اگر پرداخت یکپارچه است، خرید را انجام دهیم
                if (parameters.IsIntegrated)
                {
                    try
                    {
                        logger.LogInformation(
                            "شروع تکمیل خرید یکپارچه. سفارش: {OrderId}, کاربر: {UserId}",
                            parameters.OrderId, parameters.UserId);

                        // دریافت مبلغ کل خرید از metadata یا محاسبه آن
                        // توجه: در اینجا parameters.Amount مبلغ پرداخت شده از درگاه است (مابه‌التفاوت)
                        // باید مبلغ کل خرید را از metadata دریافت کنیم یا محاسبه کنیم

                        decimal totalPurchaseAmount = parameters.Amount; // این باید از metadata یا OrderId دریافت شود

                        // Note: در پیاده‌سازی واقعی، مبلغ کل خرید باید از سفارش یا metadata دریافت شود
                        // فعلاً فرض می‌کنیم مبلغ کل همان مبلغ پرداختی است

                        // انجام برداشت کل مبلغ خرید از کیف پول
                        var withdrawCommand = new ProcessWalletTransactionCommand(
                            parameters.UserId,
                            totalPurchaseAmount, // مبلغ کل خرید
                            parameters.Currency,
                            TransactionDirection.Out,
                            null,
                            parameters.OrderId,
                            $"خرید با شناسه {parameters.OrderId}");

                        var withdrawResult = await mediator.Send(withdrawCommand, cancellationToken);
                        result.TransactionId = withdrawResult.TransactionId;

                        logger.LogInformation(
                            "خرید یکپارچه با موفقیت تکمیل شد. تراکنش برداشت: {TransactionId}",
                            withdrawResult.TransactionId);

                        // ساخت آدرس هدایت موفق
                        redirectUrl.Append(successUrl);
                        redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                        redirectUrl.Append($"&referenceId={Uri.EscapeDataString(verifyResult.ReferenceId ?? "")}");
                        redirectUrl.Append($"&amount={totalPurchaseAmount}");
                        redirectUrl.Append($"&transactionId={withdrawResult.TransactionId}");
                        redirectUrl.Append($"&message=خرید با موفقیت انجام شد");
                    }
                    catch (InsufficientBalanceException ex)
                    {
                        logger.LogError(ex, "موجودی کیف پول برای تکمیل خرید کافی نیست. سفارش: {OrderId}", parameters.OrderId);
                        result.IsSuccessful = false;
                        result.ErrorMessage = "موجودی کیف پول برای تکمیل خرید کافی نیست";

                        redirectUrl.Append(failureUrl);
                        redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                        redirectUrl.Append($"&errorMessage={Uri.EscapeDataString("موجودی کیف پول کافی نیست")}");
                        redirectUrl.Append($"&errorCode=InsufficientBalance");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "خطا در انجام خرید یکپارچه. شناسه: {Authority}", parameters.Authority);
                        result.IsSuccessful = false;
                        result.ErrorMessage = "خطا در انجام خرید پس از پرداخت موفق";

                        redirectUrl.Append(failureUrl);
                        redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                        redirectUrl.Append($"&errorMessage={Uri.EscapeDataString(result.ErrorMessage)}");
                        redirectUrl.Append($"&errorCode=PurchaseError");
                    }
                }
                else
                {
                    // پرداخت عادی موفق (فقط شارژ کیف پول)
                    redirectUrl.Append(successUrl);
                    redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                    redirectUrl.Append($"&referenceId={Uri.EscapeDataString(verifyResult.ReferenceId ?? "")}");
                    redirectUrl.Append($"&amount={parameters.Amount}");
                    redirectUrl.Append($"&message=کیف پول با موفقیت شارژ شد");
                }
            }
            else
            {
                // پرداخت ناموفق
                result.IsSuccessful = false;
                result.ErrorMessage = verifyResult.ErrorMessage;

                redirectUrl.Append(failureUrl);
                redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                redirectUrl.Append($"&errorCode={verifyResult.ErrorCode?.ToString() ?? "Unknown"}");
                redirectUrl.Append($"&errorMessage={Uri.EscapeDataString(verifyResult.ErrorMessage ?? "خطای نامشخص")}");
            }

            result.RedirectUrl = redirectUrl.ToString();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در پردازش بازگشت از درگاه پرداخت");

            return new PaymentCallbackResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطای سیستمی در پردازش کالبک",
                RedirectUrl = configuration["Payment:FailureRedirectUrl"] ?? "/payment/failure?errorMessage=خطای سیستمی"
            };
        }
    }
}