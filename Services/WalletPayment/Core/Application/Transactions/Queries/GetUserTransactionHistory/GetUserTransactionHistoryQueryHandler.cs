using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Transactions.Queries.Common;
using WalletPayment.Domain.Entities.Transaction;


namespace WalletPayment.Application.Transactions.Queries.GetUserTransactionHistory;

public class GetUserTransactionHistoryQueryHandler(
    IWalletRepository walletRepository,
    IWalletDbContext dbContext)
    : IQueryHandler<GetUserTransactionHistoryQuery, PaginatedList<TransactionDto>>
{
    public async Task<PaginatedList<TransactionDto>> Handle(GetUserTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        // بررسی وجود کیف پول
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        // شروع از تراکنش‌های این کیف پول
        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.WalletId == wallet.Id);

        // اعمال فیلترها
        query = ApplyFilters(query, request.Filter);

        // اعمال مرتب‌سازی
        query = ApplySorting(query, request.SortBy, request.SortDesc);

        // پیاده‌سازی تبدیل به DTO
        var dtoQuery = query.Select(t => new TransactionDto
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
        });

        // صفحه‌بندی و بازگشت نتیجه
        return PaginatedList<TransactionDto>.Create(
            dtoQuery,
            request.PageNumber,
            request.PageSize);
    }

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
}