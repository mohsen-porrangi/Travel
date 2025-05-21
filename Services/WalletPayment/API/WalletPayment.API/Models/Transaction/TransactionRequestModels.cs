using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Models.Transaction;

// مدل جدید برای ایجاد تراکنش‌های کیف پول
public record CreateWalletTransactionRequest
{
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;
    public TransactionDirection Direction { get; init; } // In or Out
    public string? ReferenceId { get; init; } // برای واریز
    public string? OrderId { get; init; } // برای برداشت
    public string Description { get; init; } = string.Empty;
}

// مدل‌های قدیمی برای حفظ سازگاری
public record LegacyDepositRequest
{
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;
    public string PaymentReferenceId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record LegacyWithdrawRequest
{
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;
    public string OrderId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}