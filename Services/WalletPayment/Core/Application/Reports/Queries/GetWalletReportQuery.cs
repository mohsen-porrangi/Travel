using BuildingBlocks.CQRS;
using WalletPayment.Application.Transactions.Queries.Common;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Reports.Queries;

/// <summary>
/// نوع گزارش کیف پول
/// </summary>
public enum ReportType
{
    Transactions = 1,  // گزارش تراکنش‌ها
    Statement = 2,     // صورتحساب
    Summary = 3        // خلاصه کیف پول
}

/// <summary>
/// کوئری برای دریافت انواع گزارش‌های کیف پول
/// </summary>
public record GetWalletReportQuery : IQuery<WalletReportResponse>
{
    public Guid UserId { get; init; }
    public ReportType ReportType { get; init; } = ReportType.Transactions;
    public ExportFormat? ExportFormat { get; init; } = null;
    public TransactionFilter Filter { get; init; } = new();
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "TransactionDate";
    public bool SortDesc { get; init; } = true;
    public CurrencyCode? DisplayCurrency { get; init; } = null; // ارز نمایشی برای تبدیل مبالغ
}

/// <summary>
/// پاسخ گزارش کیف پول
/// </summary>
public record WalletReportResponse
{
    // اطلاعات کلی گزارش
    public Guid WalletId { get; init; }
    public ReportType ReportType { get; init; }
    public DateTime ReportGeneratedAt { get; init; } = DateTime.UtcNow;

    // اطلاعات صفحه‌بندی
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }

    // اطلاعات مالی و خلاصه
    public decimal? TotalBalance { get; init; }
    public string DisplayCurrency { get; init; }

    // مجموعه داده‌های گزارش (متناسب با نوع گزارش)
    public object Data { get; init; }

    // برای خروجی گرفتن از گزارش
    public byte[] FileContents { get; init; }
    public string ContentType { get; init; }
    public string FileName { get; init; }
}

/// <summary>
/// فرمت‌های خروجی برای گزارش‌ها
/// </summary>
public enum ExportFormat
{
    Csv = 1,
    Excel = 2,
    Pdf = 3
}