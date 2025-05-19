using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Common.Contracts;

public interface IPaymentGateway
{
    /// <summary>
    /// نوع درگاه پرداخت
    /// </summary>
    PaymentGatewayType GatewayType { get; }

    /// <summary>
    /// ایجاد درخواست پرداخت
    /// </summary>
    Task<PaymentRequestResult> CreatePaymentRequestAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// بررسی وضعیت پرداخت
    /// </summary>
    Task<PaymentVerificationResult> VerifyPaymentAsync(PaymentVerificationRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// استرداد وجه پرداخت شده
    /// </summary>
    Task<PaymentRefundResult> RefundPaymentAsync(PaymentRefundRequestDto request, CancellationToken cancellationToken = default);
}


/// <summary>
/// درخواست ایجاد پرداخت
/// </summary>
public record PaymentRequestDto(
    decimal Amount,
    CurrencyCode Currency,
    string Description,
    string CallbackUrl,
    string MobileNumber,
    string Email,
    Guid UserId,
    Dictionary<string, string> AdditionalData
);

/// <summary>
/// نتیجه درخواست پرداخت
/// </summary>
public record PaymentRequestResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public PaymentErrorCode? ErrorCode { get; init; }
    public string? Authority { get; init; }  // شناسه یکتای پرداخت
    public string? PaymentUrl { get; init; }  // آدرس هدایت به درگاه

    // سازنده برای حالت موفق
    public static PaymentRequestResult Success(string authority, string paymentUrl) =>
        new()
        {
            IsSuccessful = true,
            Authority = authority,
            PaymentUrl = paymentUrl
        };

    // سازنده برای حالت خطا
    public static PaymentRequestResult Failure(string errorMessage, PaymentErrorCode errorCode) =>
        new()
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
}

/// <summary>
/// درخواست بررسی وضعیت پرداخت
/// </summary>
public record PaymentVerificationRequestDto(
    string Authority,
    string Status,
    decimal Amount,
    Dictionary<string, string> AdditionalData
);

/// <summary>
/// نتیجه بررسی وضعیت پرداخت
/// </summary>
public record PaymentVerificationResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public PaymentErrorCode? ErrorCode { get; init; }
    public string? ReferenceId { get; init; }  // شناسه ارجاع درگاه پرداخت
    public decimal Amount { get; init; }
    public Dictionary<string, string> AdditionalData { get; init; } = new();

    // سازنده برای حالت موفق
    public static PaymentVerificationResult Success(string referenceId, decimal amount) =>
        new()
        {
            IsSuccessful = true,
            ReferenceId = referenceId,
            Amount = amount
        };

    // سازنده برای حالت خطا
    public static PaymentVerificationResult Failure(string errorMessage, PaymentErrorCode errorCode) =>
        new()
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
}

/// <summary>
/// درخواست استرداد وجه
/// </summary>
public record PaymentRefundRequestDto(
    string ReferenceId,
    decimal Amount,
    string Description
);

/// <summary>
/// نتیجه استرداد وجه
/// </summary>
public record PaymentRefundResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public PaymentErrorCode? ErrorCode { get; init; }
    public string? RefundTrackingId { get; init; }

    // سازنده برای حالت موفق
    public static PaymentRefundResult Success(string refundTrackingId) =>
        new()
        {
            IsSuccessful = true,
            RefundTrackingId = refundTrackingId
        };

    // سازنده برای حالت خطا
    public static PaymentRefundResult Failure(string errorMessage, PaymentErrorCode errorCode) =>
        new()
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
}

