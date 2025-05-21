using System.ComponentModel.DataAnnotations;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Models.Transaction;

// مدل جدید برای ایجاد تراکنش‌های کیف پول
public record CreateWalletTransactionRequest
{
    [Required(ErrorMessage = "مبلغ تراکنش الزامی است")]
    [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ تراکنش باید بزرگتر از صفر باشد")]
    public decimal Amount { get; init; }

    [Required(ErrorMessage = "واحد ارز الزامی است")]
    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;

    [Required(ErrorMessage = "جهت تراکنش الزامی است")]
    public TransactionDirection Direction { get; init; } // In or Out

    // برای واریز، ReferenceId الزامی است
    public string? ReferenceId { get; init; }

    // برای برداشت، OrderId می‌تواند استفاده شود
    public string? OrderId { get; init; }

    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
    public string Description { get; init; } = string.Empty;
}

// مدل‌های قدیمی برای حفظ سازگاری - منسوخ شده
[Obsolete("این مدل منسوخ شده است. لطفاً از CreateWalletTransactionRequest استفاده کنید")]
public record LegacyDepositRequest
{
    [Required(ErrorMessage = "مبلغ واریز الزامی است")]
    [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ واریز باید بزرگتر از صفر باشد")]
    public decimal Amount { get; init; }

    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;

    [Required(ErrorMessage = "شناسه مرجع پرداخت الزامی است")]
    public string PaymentReferenceId { get; init; } = string.Empty;

    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
    public string Description { get; init; } = string.Empty;
}

[Obsolete("این مدل منسوخ شده است. لطفاً از CreateWalletTransactionRequest استفاده کنید")]
public record LegacyWithdrawRequest
{
    [Required(ErrorMessage = "مبلغ برداشت الزامی است")]
    [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ برداشت باید بزرگتر از صفر باشد")]
    public decimal Amount { get; init; }

    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;

    public string OrderId { get; init; } = string.Empty;

    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
    public string Description { get; init; } = string.Empty;
}