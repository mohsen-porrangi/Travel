using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Payment.Contracts;

/// <summary>
/// سرویس مدیریت پرداخت‌های درگاه‌های خارجی و یکپارچه‌سازی با کیف پول
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// ایجاد یک درخواست پرداخت جدید
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="amount">مبلغ پرداخت</param>
    /// <param name="currency">واحد ارز</param>
    /// <param name="description">توضیحات پرداخت</param>
    /// <param name="gatewayType">نوع درگاه پرداخت</param>
    /// <param name="callbackUrl">آدرس بازگشت پس از پرداخت</param>
    /// <param name="metadata">اطلاعات اضافی (اختیاری)</param>
    /// <param name="orderId">شناسه سفارش (اختیاری)</param>
    /// <param name="cancelUrl">آدرس بازگشت در صورت لغو (اختیاری)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task<PaymentRequestResult> CreatePaymentRequestAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string description,
        PaymentGatewayType gatewayType,
        string callbackUrl,
        Dictionary<string, string>? metadata = null,
        string? orderId = null,
        string? cancelUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// بررسی وضعیت و تأیید پرداخت
    /// </summary>
    /// <param name="authority">شناسه یکتای پرداخت</param>
    /// <param name="status">وضعیت بازگشتی از درگاه</param>
    /// <param name="originalAmount">مبلغ اصلی درخواستی</param>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="automaticDeposit">شارژ خودکار کیف پول در صورت موفقیت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task<PaymentVerificationResult> VerifyPaymentAsync(
        string authority,
        string status,
        decimal originalAmount,
        Guid userId,
        bool automaticDeposit = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// درخواست استرداد وجه پرداخت شده
    /// </summary>
    /// <param name="paymentId">شناسه پرداخت</param>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="reason">دلیل استرداد</param>
    /// <param name="amount">مبلغ استرداد (در صورت خالی بودن، کل مبلغ مستر می‌شود)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task<PaymentRefundResult> RefundPaymentAsync(
        Guid paymentId,
        Guid userId,
        string reason,
        decimal? amount = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت اطلاعات یک پرداخت
    /// </summary>
    /// <param name="paymentId">شناسه پرداخت</param>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task<PaymentDetailsDto> GetPaymentDetailsAsync(
        Guid paymentId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت تاریخچه پرداخت‌های کاربر
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">اندازه صفحه</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task<PaymentHistoryResult> GetPaymentHistoryAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// انجام عملیات پرداخت یکپارچه (شارژ کیف پول و برداشت همزمان)
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="amount">مبلغ</param>
    /// <param name="currency">واحد ارز</param>
    /// <param name="description">توضیحات</param>
    /// <param name="gatewayType">نوع درگاه پرداخت</param>
    /// <param name="callbackUrl">آدرس بازگشت</param>
    /// <param name="orderId">شناسه سفارش</param>
    /// <param name="metadata">اطلاعات اضافی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task<IntegratedPaymentResult> CreateIntegratedPaymentAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string description,
        PaymentGatewayType gatewayType,
        string callbackUrl,
        string orderId,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// مدل بازگشتی اطلاعات یک پرداخت
/// </summary>
public class PaymentDetailsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public string Description { get; set; } = string.Empty;
    public PaymentGatewayType GatewayType { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ReferenceId { get; set; }
    public string? Authority { get; set; }
    public string? OrderId { get; set; }
    public string? AdditionalData { get; set; }
    public Guid? WalletTransactionId { get; set; }
}

/// <summary>
/// نتیجه تاریخچه پرداخت‌ها
/// </summary>
public class PaymentHistoryResult
{
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public List<PaymentDetailsDto> Items { get; set; } = new();
}

/// <summary>
/// نتیجه پرداخت یکپارچه
/// </summary>
public class IntegratedPaymentResult
{
    public bool IsSuccessful { get; set; }
    public string? PaymentUrl { get; set; }
    public string? Authority { get; set; }
    public Guid? PaymentId { get; set; }
    public string? ErrorMessage { get; set; }
}