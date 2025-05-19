using BuildingBlocks.Contracts.Services;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.Transactions.Commands.RefundTransaction;

public class RefundTransactionCommandHandler(
    IWalletRepository walletRepository,
    IWalletDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefundTransactionCommand, RefundTransactionResponse>
{
    public async Task<RefundTransactionResponse> Handle(RefundTransactionCommand request, CancellationToken cancellationToken)
    {
        // یافتن تراکنش اصلی که باید استرداد شود
        var originalTransaction = await dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.OriginalTransactionId, cancellationToken);

        if (originalTransaction == null)
            throw new NotFoundException("تراکنش مورد نظر یافت نشد", request.OriginalTransactionId);

        // بررسی نوع تراکنش اصلی - فقط تراکنش‌های خروجی (خرید) قابل استرداد هستند
        if (originalTransaction.Direction != TransactionDirection.Out)
            throw new BadRequestException("فقط تراکنش‌های خروجی قابل استرداد هستند");

        // بررسی وضعیت تراکنش - فقط تراکنش‌های کامل شده قابل استرداد هستند
        if (originalTransaction.Status != Domain.Entities.Enums.TransactionStatus.Completed)
            throw new BadRequestException("فقط تراکنش‌های کامل شده قابل استرداد هستند");

        // بررسی تراکنش‌های استرداد قبلی
        var previousRefunds = await dbContext.Transactions
            .Where(t => t.RelatedTransactionId == originalTransaction.Id &&
                        t.Type == Domain.Entities.Enums.TransactionType.Refund)
            .ToListAsync(cancellationToken);

        // محاسبه مبلغ قبلا استرداد شده
        decimal alreadyRefunded = previousRefunds.Sum(t => t.Amount);

        // محاسبه مبلغ استرداد فعلی
        decimal refundAmount;
        if (request.Amount.HasValue)
        {
            // استرداد جزئی
            refundAmount = request.Amount.Value;

            // بررسی مقدار معتبر
            if (refundAmount > (originalTransaction.Amount - alreadyRefunded))
                throw new BadRequestException("مبلغ استرداد درخواستی بیشتر از مبلغ قابل استرداد است",
                    $"مبلغ قابل استرداد: {originalTransaction.Amount - alreadyRefunded}");
        }
        else
        {
            // استرداد کامل (باقیمانده)
            refundAmount = originalTransaction.Amount - alreadyRefunded;

            if (refundAmount <= 0)
                throw new BadRequestException("تراکنش قبلاً به طور کامل استرداد شده است");
        }

        // دریافت کیف پول و حساب کاربر
        var wallet = await walletRepository.GetByIdAsync(originalTransaction.WalletId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول یافت نشد", originalTransaction.WalletId);

        var account = wallet.Accounts.FirstOrDefault(a => a.Id == originalTransaction.AccountInfoId);
        if (account == null)
            throw new NotFoundException("حساب مورد نظر یافت نشد", originalTransaction.AccountInfoId);

        if (!account.IsActive || !wallet.IsActive)
            throw new BadRequestException("کیف پول یا حساب غیرفعال است");

        // شروع تراکنش
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // انتشار رویداد شروع استرداد
            wallet.AddDomainEvent(new RefundInitiatedEvent(
                originalTransaction.Id,
                wallet.Id,
                account.Id,
                refundAmount,
                originalTransaction.Currency,
                request.Reason));

            // ایجاد تراکنش استرداد (واریز به حساب کاربر)
            var description = $"استرداد تراکنش {originalTransaction.Id} - {request.Reason}";
            var refundTransaction = account.Deposit(
                refundAmount,
                description,
                $"REFUND-{originalTransaction.Id}");

            // ثبت ارتباط با تراکنش اصلی
            refundTransaction.SetRelatedTransactionId(originalTransaction.Id);
            refundTransaction.SetType(Domain.Entities.Enums.TransactionType.Refund);

            // تغییر وضعیت تراکنش اصلی اگر استرداد کامل است
            bool isFullRefund = (refundAmount + alreadyRefunded) >= originalTransaction.Amount;
            if (isFullRefund)
            {
                originalTransaction.SetStatus(Domain.Entities.Enums.TransactionStatus.Refunded);
            }

            // انتشار رویداد تکمیل استرداد
            wallet.AddDomainEvent(new RefundCompletedEvent(
                refundTransaction.Id,
                originalTransaction.Id,
                wallet.Id,
                refundAmount,
                originalTransaction.Currency));

            // ذخیره تغییرات
            walletRepository.Update(wallet);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // بازگشت نتیجه
            return new RefundTransactionResponse
            {
                RefundTransactionId = refundTransaction.Id,
                OriginalTransactionId = originalTransaction.Id,
                RefundedAmount = refundAmount,
                NewAccountBalance = account.Balance,
                RefundDate = refundTransaction.TransactionDate,
                IsPartial = !isFullRefund
            };
        }
        catch
        {
            // برگشت تراکنش در صورت وقوع خطا
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}