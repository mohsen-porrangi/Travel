using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;


namespace WalletPayment.Application.Transactions.Queries.GetRefundableTransaction;

public class GetRefundableTransactionQueryHandler(IWalletDbContext dbContext)
    : IQueryHandler<GetRefundableTransactionQuery, RefundableTransactionDto>
{
    public async Task<RefundableTransactionDto> Handle(GetRefundableTransactionQuery request, CancellationToken cancellationToken)
    {
        // یافتن تراکنش اصلی
        var transaction = await dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction == null)
            throw new NotFoundException("تراکنش مورد نظر یافت نشد", request.TransactionId);

        // بررسی قابلیت استرداد
        bool isRefundable = transaction.Direction == TransactionDirection.Out &&
                          transaction.Status == Domain.Entities.Enums.TransactionStatus.Completed;

        if (!isRefundable)
            throw new BadRequestException("این تراکنش قابل استرداد نیست");

        // یافتن استردادهای قبلی
        var previousRefunds = await dbContext.Transactions
            .Where(t => t.RelatedTransactionId == transaction.Id &&
                        t.Type == TransactionType.Refund)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        // محاسبه مقادیر استرداد
        decimal alreadyRefunded = previousRefunds.Sum(t => t.Amount);
        decimal refundableAmount = transaction.Amount - alreadyRefunded;

        // تبدیل استردادهای قبلی به DTO
        var refundHistory = previousRefunds.Select(r => new RefundHistoryItemDto
        {
            RefundTransactionId = r.Id,
            Amount = r.Amount,
            RefundDate = r.TransactionDate,
            Reason = r.Description
        }).ToList();

        // بازگشت نتیجه
        return new RefundableTransactionDto
        {
            TransactionId = transaction.Id,
            WalletId = transaction.WalletId,
            AccountId = transaction.AccountInfoId,
            OriginalAmount = transaction.Amount,
            AlreadyRefundedAmount = alreadyRefunded,
            RefundableAmount = refundableAmount,
            Currency = transaction.Currency,
            TransactionDate = transaction.TransactionDate,
            Description = transaction.Description,
            IsFullyRefundable = refundableAmount >= transaction.Amount,
            HasPartialRefunds = previousRefunds.Any(),
            RefundHistory = refundHistory
        };
    }
}