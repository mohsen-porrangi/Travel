using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Domain.Entities.Transaction;

namespace WalletPayment.Application.Transactions.Queries.ExportTransactions;

public class ExportTransactionsQueryHandler(
    ISender mediator,
    IWalletRepository walletRepository,
    IWalletDbContext dbContext)
    : IQueryHandler<ExportTransactionsQuery, ExportTransactionsResponse>
{
    public async Task<ExportTransactionsResponse> Handle(ExportTransactionsQuery request, CancellationToken cancellationToken)
    {
        // بررسی وجود کیف پول
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        // دریافت تراکنش‌ها
        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.WalletId == wallet.Id);

        // اعمال فیلترها - استفاده از همان منطق GetUserTransactionHistoryQueryHandler
        if (request.Filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= request.Filter.StartDate.Value);

        if (request.Filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= request.Filter.EndDate.Value);

        if (request.Filter.Direction.HasValue)
            query = query.Where(t => t.Direction == request.Filter.Direction.Value);

        if (request.Filter.Type.HasValue)
            query = query.Where(t => t.Type == request.Filter.Type.Value);

        if (request.Filter.Status.HasValue)
            query = query.Where(t => t.Status == request.Filter.Status.Value);

        if (request.Filter.Currency.HasValue)
            query = query.Where(t => t.Currency == request.Filter.Currency.Value);

        if (request.Filter.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= request.Filter.MinAmount.Value);

        if (request.Filter.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= request.Filter.MaxAmount.Value);

        if (request.Filter.IsCredit.HasValue)
            query = query.Where(t => t.IsCredit == request.Filter.IsCredit.Value);

        // مرتب‌سازی بر اساس تاریخ
        query = query.OrderByDescending(t => t.TransactionDate);

        // دریافت داده‌ها
        var transactions = await query.ToListAsync(cancellationToken);

        // ایجاد فایل خروجی بر اساس فرمت درخواستی
        byte[] fileContents;
        string fileName;
        string contentType;

        switch (request.Format)
        {
            case ExportFormat.Csv:
                fileContents = GenerateCsv(transactions);
                fileName = $"transactions_{DateTime.Now:yyyyMMdd}.csv";
                contentType = "text/csv";
                break;

            case ExportFormat.Excel:
                fileContents = GenerateExcel(transactions);
                fileName = $"transactions_{DateTime.Now:yyyyMMdd}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                break;

            case ExportFormat.Pdf:
                fileContents = GeneratePdf(transactions);
                fileName = $"transactions_{DateTime.Now:yyyyMMdd}.pdf";
                contentType = "application/pdf";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(request.Format), request.Format, "فرمت خروجی پشتیبانی نمی‌شود");
        }

        return new ExportTransactionsResponse
        {
            FileContents = fileContents,
            FileName = fileName,
            ContentType = contentType
        };
    }

    private byte[] GenerateCsv(List<Transaction> transactions)
    {
        var csv = new StringBuilder();

        // ستون‌های هدر
        csv.AppendLine("ID,Date,Amount,Direction,Type,Status,Currency,Description,IsCredit,ReferenceId,OrderId");

        // داده‌ها
        foreach (var t in transactions)
        {
            var direction = t.Direction == TransactionDirection.In ? "In" : "Out";
            var date = t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
            var amount = t.Amount.ToString(CultureInfo.InvariantCulture);
            var description = t.Description?.Replace(",", " ").Replace("\"", "'") ?? "";

            csv.AppendLine($"{t.Id},{date},{amount},{direction},{t.Type},{t.Status},{t.Currency},{description},{t.IsCredit},{t.PaymentReferenceId},{t.OrderId}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] GenerateExcel(List<Transaction> transactions)
    {
        // در یک پروژه واقعی، از یک کتابخانه مثل EPPlus یا NPOI استفاده می‌شود
        // برای نمونه، همان CSV را برمی‌گردانیم
        return GenerateCsv(transactions);
    }

    private byte[] GeneratePdf(List<Transaction> transactions)
    {
        // در یک پروژه واقعی، از یک کتابخانه مثل iTextSharp یا PDFsharp استفاده می‌شود
        // برای نمونه، همان CSV را برمی‌گردانیم
        return GenerateCsv(transactions);
    }
}