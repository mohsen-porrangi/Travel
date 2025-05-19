using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.ExternalServices.PaymentGateway;

public class SandboxGateway : IPaymentGateway
{
    private readonly ILogger<SandboxGateway> _logger;
    private readonly string _baseUrl;

    public SandboxGateway(
        IConfiguration configuration,
        ILogger<SandboxGateway> logger)
    {
        _logger = logger;
        _baseUrl = configuration["Payment:Sandbox:BaseUrl"] ?? "http://localhost:5000";
    }

    public PaymentGatewayType GatewayType => PaymentGatewayType.Sandbox;

    public Task<PaymentRequestResult> CreatePaymentRequestAsync(
        PaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // ایجاد یک شناسه تصادفی برای شبیه‌سازی درگاه
        var authority = $"SBX-{Guid.NewGuid():N}";

        // ساخت آدرس شبیه‌سازی شده برای هدایت کاربر
        var paymentUrl = $"{_baseUrl}/sandbox-payment?authority={authority}&amount={request.Amount}&callback={Uri.EscapeDataString(request.CallbackUrl)}";

        _logger.LogInformation(
            "درخواست پرداخت Sandbox با موفقیت ایجاد شد. شناسه: {Authority}, مبلغ: {Amount}",
            authority,
            request.Amount);

        return Task.FromResult(PaymentRequestResult.Success(authority, paymentUrl));
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(
        PaymentVerificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // بررسی وضعیت بازگشتی
        if (request.Status != "OK" && request.Status != "success")
        {
            _logger.LogWarning(
                "پرداخت Sandbox توسط کاربر لغو شد. شناسه: {Authority}, وضعیت: {Status}",
                request.Authority,
                request.Status);

            return Task.FromResult(PaymentVerificationResult.Failure(
                "پرداخت توسط کاربر لغو شد",
                PaymentErrorCode.CanceledByUser));
        }

        // ایجاد یک شناسه مرجع تصادفی
        var referenceId = $"SBX-REF-{DateTime.Now:yyyyMMddHHmmss}";

        _logger.LogInformation(
            "تأیید پرداخت Sandbox با موفقیت انجام شد. شناسه ارجاع: {ReferenceId}, مبلغ: {Amount}",
            referenceId,
            request.Amount);

        return Task.FromResult(PaymentVerificationResult.Success(referenceId, request.Amount));
    }

    public Task<PaymentRefundResult> RefundPaymentAsync(
        PaymentRefundRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // ایجاد یک شناسه پیگیری استرداد تصادفی
        var refundTrackingId = $"SBX-REFUND-{DateTime.Now:yyyyMMddHHmmss}";

        _logger.LogInformation(
            "استرداد وجه Sandbox با موفقیت انجام شد. شناسه استرداد: {RefundTrackingId}, مبلغ: {Amount}",
            refundTrackingId,
            request.Amount);

        return Task.FromResult(PaymentRefundResult.Success(refundTrackingId));
    }
}