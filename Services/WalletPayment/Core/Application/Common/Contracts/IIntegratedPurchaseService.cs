using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Payment.Contracts;

/// <summary>
/// رابط سرویس یکپارچه خرید و پرداخت
/// </summary>
public interface IIntegratedPurchaseService
{
    /// <summary>
    /// ایجاد درخواست پرداخت یکپارچه
    /// </summary>
    Task<IntegratedPurchaseResult> CreateIntegratedPurchaseRequestAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string description,
        PaymentGatewayType gatewayType,
        string callbackUrl,
        string orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تکمیل فرآیند خرید یکپارچه
    /// </summary>
    Task<IntegratedPurchaseCompletionResult> CompleteIntegratedPurchaseAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string orderId,
        string paymentReferenceId,
        string description,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// نتیجه تکمیل خرید یکپارچه
/// </summary>
public class IntegratedPurchaseCompletionResult
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid DepositTransactionId { get; set; }
    public Guid WithdrawTransactionId { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingBalance { get; set; }
}

/// <summary>
/// نتیجه درخواست خرید یکپارچه
/// </summary>
public class IntegratedPurchaseResult
{
    public bool IsSuccessful { get; set; }
    public string? PaymentUrl { get; set; }
    public string? Authority { get; set; }
    public string? ErrorMessage { get; set; }
}