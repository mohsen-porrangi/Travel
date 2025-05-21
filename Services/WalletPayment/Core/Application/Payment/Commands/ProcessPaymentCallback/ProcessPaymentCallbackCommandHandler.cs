using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Application.Transactions.Commands.IntegratedPurchase;

namespace WalletPayment.Application.Payment.Commands.ProcessPaymentCallback;

public class ProcessPaymentCallbackCommandHandler :
    ICommandHandler<ProcessPaymentCallbackCommand, PaymentCallbackResult>
{
    private readonly IPaymentService _paymentService;
    private readonly ISender _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProcessPaymentCallbackCommandHandler> _logger;

    public ProcessPaymentCallbackCommandHandler(
        IPaymentService paymentService,
        ISender mediator,
        IConfiguration configuration,
        ILogger<ProcessPaymentCallbackCommandHandler> logger)
    {
        _paymentService = paymentService;
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

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
                successUrl = _configuration["Payment:IntegratedSuccessRedirectUrl"] ?? "/payment/integrated/success";
                failureUrl = _configuration["Payment:IntegratedFailureRedirectUrl"] ?? "/payment/integrated/failure";
            }
            else
            {
                successUrl = _configuration["Payment:SuccessRedirectUrl"] ?? "/payment/success";
                failureUrl = _configuration["Payment:FailureRedirectUrl"] ?? "/payment/failure";
            }

            // تأیید پرداخت
            var verifyResult = await _paymentService.VerifyPaymentAsync(
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
                        // انجام عملیات خرید یکپارچه
                        var purchaseCommand = new IntegratedPurchaseCommand(
                            parameters.UserId,
                            parameters.Amount,
                            parameters.Currency,
                            parameters.OrderId,
                            $"خرید با شناسه {parameters.OrderId}",
                            verifyResult.ReferenceId
                        );

                        var purchaseResult = await _mediator.Send(purchaseCommand, cancellationToken);
                        result.TransactionId = purchaseResult.WithdrawTransactionId;

                        // ساخت آدرس هدایت موفق
                        redirectUrl.Append(successUrl);
                        redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                        redirectUrl.Append($"&referenceId={Uri.EscapeDataString(verifyResult.ReferenceId ?? "")}");
                        redirectUrl.Append($"&amount={parameters.Amount}");
                        redirectUrl.Append($"&transactionId={purchaseResult.WithdrawTransactionId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "خطا در انجام خرید یکپارچه. شناسه: {Authority}", parameters.Authority);
                        result.IsSuccessful = false;
                        result.ErrorMessage = "خطا در انجام خرید پس از پرداخت موفق";

                        redirectUrl.Append(failureUrl);
                        redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                        redirectUrl.Append($"&errorMessage={Uri.EscapeDataString(result.ErrorMessage)}");
                    }
                }
                else
                {
                    // پرداخت عادی موفق
                    redirectUrl.Append(successUrl);
                    redirectUrl.Append($"?orderId={Uri.EscapeDataString(parameters.OrderId ?? "")}");
                    redirectUrl.Append($"&referenceId={Uri.EscapeDataString(verifyResult.ReferenceId ?? "")}");
                    redirectUrl.Append($"&amount={parameters.Amount}");
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
            _logger.LogError(ex, "خطا در پردازش بازگشت از درگاه پرداخت");

            return new PaymentCallbackResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطای سیستمی در پردازش کالبک",
                RedirectUrl = _configuration["Payment:FailureRedirectUrl"] ?? "/payment/failure?errorMessage=خطای سیستمی"
            };
        }
    }
}