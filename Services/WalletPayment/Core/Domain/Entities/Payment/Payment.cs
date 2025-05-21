using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using WalletPayment.Domain.Entities;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Domain.Entities.Payment;

public class Payment : EntityWithDomainEvents, ISoftDelete
{
    public Guid WalletId { get; private set; }
    public Guid? AccountInfoId { get; private set; }  // اختیاری - در پرداخت‌های مستقیم خالی است
    public decimal Amount { get; private set; }
    public CurrencyCode Currency { get; private set; }
    public PaymentGatewayType GatewayType { get; private set; }
    public string Description { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? Authority { get; private set; }
    public string? ReferenceId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? TransactionId { get; private set; }  // ارتباط با تراکنش کیف پول
    public string? CallbackUrl { get; private set; }
    public string? AdditionalData { get; private set; }  // داده‌های اضافی به صورت JSON
    public string? ErrorMessage { get; private set; }
    public PaymentErrorCode? ErrorCode { get; private set; }
    public string? RefundTrackingId { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public Transaction.Transaction? Transaction { get; private set; }

    
    private Payment() { }
     
    public Payment(
        Guid walletId,
        Guid? accountInfoId,
        decimal amount,
        CurrencyCode currency,
        PaymentGatewayType gatewayType,
        string description,
        string? callbackUrl)
    {
        Id = Guid.NewGuid();
        WalletId = walletId;
        AccountInfoId = accountInfoId;
        Amount = amount;
        Currency = currency;
        GatewayType = gatewayType;
        Description = description;
        Status = PaymentStatus.Pending;
        CallbackUrl = callbackUrl;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        IsDeleted = false;
    }

    // متد برای ذخیره Authority درگاه پرداخت
    public void SetAuthority(string authority)
    {
        Authority = authority;
        UpdatedAt = DateTime.UtcNow;
    }

    // متد برای ثبت پرداخت موفق
    public void MarkAsPaid(string referenceId, DateTime? paidAt = null)
    {
        Status = PaymentStatus.Paid;
        ReferenceId = referenceId;
        PaidAt = paidAt ?? DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // متد برای ثبت تأیید پرداخت
    public void MarkAsVerified(Guid transactionId)
    {
        Status = PaymentStatus.Verified;
        TransactionId = transactionId;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // متد برای ثبت پرداخت ناموفق
    public void MarkAsFailed(string errorMessage, PaymentErrorCode errorCode)
    {
        Status = PaymentStatus.Failed;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        UpdatedAt = DateTime.UtcNow;
    }

    // متد برای ثبت لغو پرداخت
    public void MarkAsCancelled()
    {
        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    // متد برای ثبت استرداد وجه
    public void MarkAsRefunded(string refundTrackingId)
    {
        Status = PaymentStatus.Refunded;
        RefundTrackingId = refundTrackingId;
        UpdatedAt = DateTime.UtcNow;
    }

    // متد برای ذخیره داده‌های اضافی
    public void SetAdditionalData(string additionalDataJson)
    {
        AdditionalData = additionalDataJson;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PaymentStatus
{
    Pending = 1,    // در انتظار پرداخت
    Paid = 2,       // پرداخت شده (هنوز تأیید نشده)
    Verified = 3,   // تأیید شده
    Failed = 4,     // ناموفق
    Cancelled = 5,  // لغو شده
    Refunded = 6    // استرداد شده
}