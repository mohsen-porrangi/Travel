using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Common.Contracts;

/// <summary>
/// سرویس مدیریت تراکنش‌های پرداخت
/// </summary>
public interface IPaymentTransactionService
{
    /// <summary>
    /// ایجاد تراکنش پرداخت جدید
    /// </summary>
    Task<Guid> CreatePaymentTransactionAsync(
        Guid userId,
        decimal amount,
        PaymentGatewayType gatewayType,
        string description,
        string? orderId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی وضعیت تراکنش پرداخت
    /// </summary>
    Task<bool> UpdatePaymentTransactionStatusAsync(
        Guid transactionId,
        Domain.Entities.Enums.PaymentTransactionStatus status,
        string? referenceId = null,
        string? gatewayResponse = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت اطلاعات تراکنش پرداخت
    /// </summary>
    Task<PaymentTransactionDto?> GetPaymentTransactionAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت اطلاعات تراکنش پرداخت با توکن
    /// </summary>
    Task<PaymentTransactionDto?> GetPaymentTransactionByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تکمیل تراکنش موفق و شارژ کیف پول
    /// </summary>
    Task<bool> CompleteSuccessfulPaymentAsync(
        Guid transactionId,
        string referenceId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// وضعیت تراکنش پرداخت
/// </summary>
public enum PaymentTransactionStatus
{
    Pending = 1,
    Processing = 2,
    Successful = 3,
    Failed = 4,
    Canceled = 5,
    Refunded = 6
}

/// <summary>
/// مدل داده تراکنش پرداخت
/// </summary>
public class PaymentTransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentGatewayType GatewayType { get; set; }
    public string GatewayToken { get; set; } = string.Empty;
    public Domain.Entities.Enums.PaymentTransactionStatus Status { get; set; }
    public string? ReferenceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? WalletTransactionId { get; set; }
}