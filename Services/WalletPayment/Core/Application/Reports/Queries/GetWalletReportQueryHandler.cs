using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Transactions.Queries.Common;
using WalletPayment.Domain.Entities.Transaction;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Application.Reports.Models;

namespace WalletPayment.Application.Reports.Queries.GetWalletReport;

/// <summary>
/// هندلر برای پردازش انواع گزارش‌های کیف پول
/// </summary>
public class GetWalletReportQueryHandler : IQueryHandler<GetWalletReportQuery, WalletReportResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletDbContext _dbContext;
    private readonly ICurrencyExchangeService _currencyExchangeService;
    private readonly ILogger<GetWalletReportQueryHandler> _logger;

    public GetWalletReportQueryHandler(
        IWalletRepository walletRepository,
        IWalletDbContext dbContext,
        ICurrencyExchangeService currencyExchangeService,
        ILogger<GetWalletReportQueryHandler> logger)
    {
        _walletRepository = walletRepository;
        _dbContext = dbContext;
        _currencyExchangeService = currencyExchangeService;
        _logger = logger;
    }

    public async Task<WalletReportResponse> Handle(GetWalletReportQuery request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await _walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        // انتخاب ارز نمایشی
        var displayCurrency = request.DisplayCurrency ?? CurrencyCode.IRR;

        // اجرای روش مناسب براساس نوع گزارش درخواستی
        switch (request.ReportType)
        {
            case ReportType.Transactions:
                return await GetTransactionsReport(wallet.Id, request, displayCurrency, cancellationToken);

            case ReportType.Statement:
                return await GetStatementReport(wallet.Id, request, displayCurrency, cancellationToken);

            case ReportType.Summary:
                return await GetSummaryReport(wallet.Id, request, displayCurrency, cancellationToken);

            default:
                throw new BadRequestException($"نوع گزارش نامعتبر: {request.ReportType}");
        }
    }

    /// <summary>
    /// دریافت گزارش تراکنش‌ها
    /// </summary>
    private async Task<WalletReportResponse> GetTransactionsReport(
        Guid walletId,
        GetWalletReportQuery request,
        CurrencyCode displayCurrency,
        CancellationToken cancellationToken)
    {
        // شروع از تراکنش‌های این کیف پول
        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.WalletId == walletId);

        // اعمال فیلترها
        query = ApplyFilters(query, request.Filter);

        // اعمال مرتب‌سازی
        query = ApplySorting(query, request.SortBy, request.SortDesc);

        // محاسبه تعداد کل برای صفحه‌بندی
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // اگر خروجی گرفتن درخواست شده باشد
        if (request.ExportFormat.HasValue)
        {
            var allTransactions = await query.ToListAsync(cancellationToken);
            var fileResult = GenerateExportFile(allTransactions, request.ExportFormat.Value, displayCurrency);

            return new WalletReportResponse
            {
                WalletId = walletId,
                ReportType = ReportType.Transactions,
                TotalItems = totalCount,
                TotalPages = totalPages,
                CurrentPage = 1,
                PageSize = totalCount,
                DisplayCurrency = displayCurrency.ToString(),
                FileContents = fileResult.FileContents,
                ContentType = fileResult.ContentType,
                FileName = fileResult.FileName
            };
        }

        // صفحه‌بندی برای نمایش عادی
        var pagedTransactions = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // تبدیل به DTO
        var transactionDtos = pagedTransactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            WalletId = t.WalletId,
            AccountId = t.AccountInfoId,
            RelatedTransactionId = t.RelatedTransactionId,
            Amount = t.Amount,
            Direction = t.Direction,
            Type = t.Type,
            Status = t.Status,
            TransactionDate = t.TransactionDate,
            Currency = t.Currency,
            Description = t.Description,
            IsCredit = t.IsCredit,
            DueDate = t.DueDate,
            PaymentReferenceId = t.PaymentReferenceId,
            OrderId = t.OrderId
        }).ToList();

        return new WalletReportResponse
        {
            WalletId = walletId,
            ReportType = ReportType.Transactions,
            TotalItems = totalCount,
            TotalPages = totalPages,
            CurrentPage = request.PageNumber,
            PageSize = request.PageSize,
            DisplayCurrency = displayCurrency.ToString(),
            Data = transactionDtos
        };
    }

    /// <summary>
    /// دریافت گزارش صورتحساب
    /// </summary>
    private async Task<WalletReportResponse> GetStatementReport(
        Guid walletId,
        GetWalletReportQuery request,
        CurrencyCode displayCurrency,
        CancellationToken cancellationToken)
    {
        // یافتن حساب با ارز درخواستی
        var account = (await _walletRepository.GetByIdAsync(walletId, cancellationToken))
            .Accounts.FirstOrDefault(a => a.Currency == displayCurrency);

        if (account == null)
            throw new NotFoundException($"حساب با ارز {displayCurrency} برای کاربر یافت نشد", walletId);

        // تعیین بازه زمانی
        var startDate = request.Filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.Filter.EndDate ?? DateTime.UtcNow;

        // محاسبه موجودی ابتدایی (قبل از شروع بازه گزارش)
        var openingBalanceTransactions = await _dbContext.Transactions
            .Where(t => t.AccountInfoId == account.Id && t.TransactionDate < startDate)
            .ToListAsync(cancellationToken);

        decimal openingBalance = CalculateBalance(openingBalanceTransactions);

        // دریافت تراکنش‌های بازه زمانی
        var periodTransactions = await _dbContext.Transactions
            .Where(t => t.AccountInfoId == account.Id &&
                  t.TransactionDate >= startDate &&
                  t.TransactionDate <= endDate)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        // محاسبه مقادیر خلاصه
        var totalDeposits = periodTransactions
            .Where(t => t.Direction == TransactionDirection.In)
            .Sum(t => t.Amount);

        var totalWithdrawals = periodTransactions
            .Where(t => t.Direction == TransactionDirection.Out)
            .Sum(t => t.Amount);

        decimal closingBalance = openingBalance + totalDeposits - totalWithdrawals;

        // ساخت ورودی‌های صورتحساب با موجودی لحظه‌ای
        decimal runningBalance = openingBalance;
        var entries = new List<StatementEntryDto>();

        foreach (var transaction in periodTransactions)
        {
            // بروزرسانی موجودی لحظه‌ای
            if (transaction.Direction == TransactionDirection.In)
                runningBalance += transaction.Amount;
            else
                runningBalance -= transaction.Amount;

            // ساخت ورودی صورتحساب
            entries.Add(new StatementEntryDto
            {
                Date = transaction.TransactionDate,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Direction = transaction.Direction,
                RunningBalance = runningBalance,
                Type = transaction.Type,
                Reference = transaction.PaymentReferenceId ?? transaction.OrderId ?? transaction.Id.ToString()
            });
        }

        var statementData = new AccountStatementData
        {
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            TotalDeposits = totalDeposits,
            TotalWithdrawals = totalWithdrawals,
            TotalTransactions = periodTransactions.Count,
            StatementStartDate = startDate,
            StatementEndDate = endDate,
            Currency = displayCurrency,
            Entries = entries
        };

        // اگر خروجی گرفتن درخواست شده باشد
        if (request.ExportFormat.HasValue)
        {
            var fileResult = GenerateStatementExportFile(statementData, request.ExportFormat.Value);

            return new WalletReportResponse
            {
                WalletId = walletId,
                ReportType = ReportType.Statement,
                TotalItems = periodTransactions.Count,
                TotalPages = 1,
                CurrentPage = 1,
                PageSize = periodTransactions.Count,
                TotalBalance = closingBalance,
                DisplayCurrency = displayCurrency.ToString(),
                FileContents = fileResult.FileContents,
                ContentType = fileResult.ContentType,
                FileName = fileResult.FileName
            };
        }

        return new WalletReportResponse
        {
            WalletId = walletId,
            ReportType = ReportType.Statement,
            TotalItems = periodTransactions.Count,
            TotalPages = 1,
            CurrentPage = 1,
            PageSize = periodTransactions.Count,
            TotalBalance = closingBalance,
            DisplayCurrency = displayCurrency.ToString(),
            Data = statementData
        };
    }

    /// <summary>
    /// دریافت گزارش خلاصه کیف پول
    /// </summary>
    private async Task<WalletReportResponse> GetSummaryReport(
        Guid walletId,
        GetWalletReportQuery request,
        CurrencyCode displayCurrency,
        CancellationToken cancellationToken)
    {
        // دریافت کیف پول با اطلاعات اعتبار
        var wallet = await _walletRepository.GetByUserIdWithCreditHistoryAsync(request.UserId, cancellationToken);

        // بررسی وضعیت سررسید اعتبار
        wallet.CheckCreditDueDate();

        // خلاصه حساب‌ها
        var accountSummaries = wallet.Accounts
            .Where(a => !a.IsDeleted)
            .Select(a => new AccountSummaryDto
            {
                AccountId = a.Id,
                AccountNumber = a.CurrencyAccountCode,
                Currency = a.Currency,
                Balance = a.Balance,
                IsActive = a.IsActive
            })
            .ToList();

        // خلاصه اعتبار
        bool hasCredit = wallet.CreditLimit > 0 && wallet.CreditDueDate.HasValue;
        bool isOverdue = hasCredit && wallet.CreditDueDate.Value < DateTime.UtcNow;

        var creditSummary = new CreditSummaryDto
        {
            CreditLimit = wallet.CreditLimit,
            AvailableCredit = wallet.CreditBalance,
            UsedCredit = wallet.CreditLimit - wallet.CreditBalance,
            DueDate = wallet.CreditDueDate,
            HasActiveCredit = hasCredit,
            IsOverdue = isOverdue
        };

        // آمار تراکنش‌ها
        var lastMonth = DateTime.UtcNow.AddMonths(-1);

        // استفاده از اجرای یک کوئری واحد برای کاهش تعداد درخواست‌ها به دیتابیس
        var transactionStats = await _dbContext.Transactions
            .Where(t => t.WalletId == wallet.Id)
            .GroupBy(t => 1) // گروه‌بندی همه با هم
            .Select(g => new
            {
                TotalCount = g.Count(),
                LastMonthCount = g.Count(t => t.TransactionDate >= lastMonth),
                TotalAmount = g.Sum(t => t.Amount),
                LastMonthAmount = g.Sum(t => t.TransactionDate >= lastMonth ? t.Amount : 0),
                LastTransactionDate = g.Max(t => t.TransactionDate)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var transactionSummary = new TransactionsSummaryDto
        {
            TotalCount = transactionStats?.TotalCount ?? 0,
            LastMonthCount = transactionStats?.LastMonthCount ?? 0,
            TotalAmount = transactionStats?.TotalAmount ?? 0,
            LastMonthAmount = transactionStats?.LastMonthAmount ?? 0,
            LastTransactionDate = transactionStats?.LastTransactionDate
        };

        // محاسبه موجودی کل در ارز نمایشی
        decimal totalBalance = 0;
        foreach (var account in wallet.Accounts.Where(a => a.IsActive && !a.IsDeleted))
        {
            if (account.Currency == displayCurrency)
            {
                totalBalance += account.Balance;
            }
            else
            {
                var rate = await _currencyExchangeService.GetExchangeRateAsync(account.Currency, displayCurrency);
                totalBalance += account.Balance * rate;
            }
        }

        var summaryData = new WalletSummaryDto
        {
            WalletId = wallet.Id,
            IsActive = wallet.IsActive,
            TotalAccounts = accountSummaries.Count,
            Accounts = accountSummaries,
            Credit = creditSummary,
            Transactions = transactionSummary,
            TotalBalance = totalBalance,
            DisplayCurrency = displayCurrency
        };

        return new WalletReportResponse
        {
            WalletId = walletId,
            ReportType = ReportType.Summary,
            TotalItems = 1,
            TotalPages = 1,
            CurrentPage = 1,
            PageSize = 1,
            TotalBalance = totalBalance,
            DisplayCurrency = displayCurrency.ToString(),
            Data = summaryData
        };
    }

    #region Helper Methods

    /// <summary>
    /// اعمال فیلترها روی کوئری تراکنش‌ها
    /// </summary>
    private static IQueryable<Transaction> ApplyFilters(
        IQueryable<Transaction> query,
        TransactionFilter filter)
    {
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value);

        if (filter.Direction.HasValue)
            query = query.Where(t => t.Direction == filter.Direction.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Currency.HasValue)
            query = query.Where(t => t.Currency == filter.Currency.Value);

        if (filter.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

        if (filter.IsCredit.HasValue)
            query = query.Where(t => t.IsCredit == filter.IsCredit.Value);

        if (!string.IsNullOrWhiteSpace(filter.OrderId))
            query = query.Where(t => t.OrderId == filter.OrderId);

        if (!string.IsNullOrWhiteSpace(filter.ReferenceId))
            query = query.Where(t => t.PaymentReferenceId == filter.ReferenceId);

        return query;
    }

    /// <summary>
    /// اعمال مرتب‌سازی روی کوئری تراکنش‌ها
    /// </summary>
    private static IQueryable<Transaction> ApplySorting(
        IQueryable<Transaction> query,
        string sortBy,
        bool sortDesc)
    {
        // نگاشت نام فیلد به Expression مناسب
        Expression<Func<Transaction, object>> keySelector = sortBy.ToLower() switch
        {
            "amount" => t => t.Amount,
            "date" => t => t.TransactionDate,
            "transactiondate" => t => t.TransactionDate,
            "type" => t => t.Type,
            "direction" => t => t.Direction,
            "status" => t => t.Status,
            "currency" => t => t.Currency,
            _ => t => t.TransactionDate // پیش‌فرض
        };

        // اعمال مرتب‌سازی
        return sortDesc
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    /// <summary>
    /// محاسبه موجودی از لیست تراکنش‌ها
    /// </summary>
    private static decimal CalculateBalance(IEnumerable<Transaction> transactions)
    {
        decimal balance = 0;

        foreach (var transaction in transactions)
        {
            if (transaction.Direction == TransactionDirection.In)
                balance += transaction.Amount;
            else
                balance -= transaction.Amount;
        }

        return balance;
    }

    /// <summary>
    /// تولید فایل خروجی برای گزارش تراکنش‌ها
    /// </summary>
    private (byte[] FileContents, string ContentType, string FileName) GenerateExportFile(
        List<Transaction> transactions,
        ExportFormat format,
        CurrencyCode displayCurrency)
    {
        switch (format)
        {
            case ExportFormat.Csv:
                return GenerateCsvFile(transactions);
            case ExportFormat.Excel:
                return GenerateExcelFile(transactions);
            case ExportFormat.Pdf:
                return GeneratePdfFile(transactions);
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "فرمت خروجی پشتیبانی نمی‌شود");
        }
    }

    /// <summary>
    /// تولید فایل خروجی برای گزارش صورتحساب
    /// </summary>
    private (byte[] FileContents, string ContentType, string FileName) GenerateStatementExportFile(
        AccountStatementData statementData,
        ExportFormat format)
    {
        // پیاده‌سازی تولید خروجی برای صورتحساب
        return format switch
        {
            ExportFormat.Csv => GenerateStatementCsvFile(statementData),
            ExportFormat.Excel => GenerateStatementExcelFile(statementData),
            ExportFormat.Pdf => GenerateStatementPdfFile(statementData),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "فرمت خروجی پشتیبانی نمی‌شود")
        };
    }

    private (byte[] FileContents, string ContentType, string FileName) GenerateCsvFile(List<Transaction> transactions)
    {
        var csv = new StringBuilder();

        // ستون‌های هدر
        csv.AppendLine("ID,Date,Amount,Direction,Type,Status,Currency,Description,IsCredit,ReferenceId,OrderId");

        // داده‌ها
        foreach (var t in transactions)
        {
            var direction = t.Direction == TransactionDirection.In ? "In" : "Out";
            var date = t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
            var amount = t.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var description = t.Description?.Replace(",", " ").Replace("\"", "'") ?? "";

            csv.AppendLine($"{t.Id},{date},{amount},{direction},{t.Type},{t.Status},{t.Currency},{description},{t.IsCredit},{t.PaymentReferenceId},{t.OrderId}");
        }

        return (Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"transactions_{DateTime.Now:yyyyMMdd}.csv");
    }

    private (byte[] FileContents, string ContentType, string FileName) GenerateExcelFile(List<Transaction> transactions)
    {
        // در یک پروژه واقعی، از یک کتابخانه مثل EPPlus یا NPOI استفاده می‌شود
        // برای فعلاً، همان CSV را برمی‌گردانیم
        return GenerateCsvFile(transactions);
    }

    private (byte[] FileContents, string ContentType, string FileName) GeneratePdfFile(List<Transaction> transactions)
    {
        // در یک پروژه واقعی، از یک کتابخانه مثل iTextSharp یا PDFsharp استفاده می‌شود
        // برای فعلاً، همان CSV را برمی‌گردانیم
        return GenerateCsvFile(transactions);
    }

    private (byte[] FileContents, string ContentType, string FileName) GenerateStatementCsvFile(AccountStatementData statementData)
    {
        // پیاده‌سازی تولید فایل CSV برای صورتحساب
        // برای این مثال، همان منطق GenerateCsvFile را با تغییرات مناسب استفاده می‌کنیم
        var csv = new StringBuilder();

        // اطلاعات هدر صورتحساب
        csv.AppendLine($"Account Statement Report,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Period,{statementData.StatementStartDate:yyyy-MM-dd} to {statementData.StatementEndDate:yyyy-MM-dd}");
        csv.AppendLine($"Currency,{statementData.Currency}");
        csv.AppendLine($"Opening Balance,{statementData.OpeningBalance}");
        csv.AppendLine($"Closing Balance,{statementData.ClosingBalance}");
        csv.AppendLine($"Total Deposits,{statementData.TotalDeposits}");
        csv.AppendLine($"Total Withdrawals,{statementData.TotalWithdrawals}");
        csv.AppendLine();

        // ستون‌های هدر
        csv.AppendLine("Date,Description,Amount,Direction,Balance,Type,Reference");

        // داده‌های تراکنش‌ها
        foreach (var entry in statementData.Entries)
        {
            var direction = entry.Direction == TransactionDirection.In ? "In" : "Out";
            var date = entry.Date.ToString("yyyy-MM-dd HH:mm:ss");
            var amount = entry.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var balance = entry.RunningBalance.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var description = entry.Description?.Replace(",", " ").Replace("\"", "'") ?? "";

            csv.AppendLine($"{date},{description},{amount},{direction},{balance},{entry.Type},{entry.Reference}");
        }

        return (Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"statement_{DateTime.Now:yyyyMMdd}.csv");
    }

    private (byte[] FileContents, string ContentType, string FileName) GenerateStatementExcelFile(AccountStatementData statementData)
    {
        // برای فعلاً، همان CSV را برمی‌گردانیم
        return GenerateStatementCsvFile(statementData);
    }

    private (byte[] FileContents, string ContentType, string FileName) GenerateStatementPdfFile(AccountStatementData statementData)
    {
        // برای فعلاً، همان CSV را برمی‌گردانیم
        return GenerateStatementCsvFile(statementData);
    }

    #endregion
}

