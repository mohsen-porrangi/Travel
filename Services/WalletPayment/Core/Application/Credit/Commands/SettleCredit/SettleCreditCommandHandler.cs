using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using System.Transactions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;
using TransactionStatus = WalletPayment.Domain.Entities.Enums.TransactionStatus;

namespace WalletPayment.Application.Credit.Commands.SettleCredit;

public class SettleCreditCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SettleCreditCommand, SettleCreditResponse>
{
    public async Task<SettleCreditResponse> Handle(SettleCreditCommand request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        // بررسی وجود اعتبار فعال
        if (wallet.CreditLimit <= 0 || !wallet.CreditDueDate.HasValue)
            throw new BadRequestException("کاربر دارای اعتبار فعال نیست");

        // محاسبه مبلغ باقیمانده برای تسویه
        decimal amountToSettle = wallet.CreditLimit - wallet.CreditBalance;
        if (amountToSettle <= 0)
            throw new BadRequestException("مبلغی برای تسویه وجود ندارد");

        // ایجاد تراکنش تسویه
        var transaction = new Domain.Entities.Transaction.Transaction(
            Guid.Empty, // حساب پیش‌فرض
            wallet.Id,
            amountToSettle,
            TransactionDirection.In,
            TransactionType.CreditSettlement,
            TransactionStatus.Completed,
            CurrencyCode.IRR, // ارز پیش‌فرض
            "تسویه اعتبار",
            false,
            null,
            request.PaymentReferenceId,
            null
        );

        // ذخیره تراکنش
        await walletRepository.AddTransactionAsync(transaction, cancellationToken);

        // تسویه اعتبار
        wallet.SettleCredit(transaction.Id);

        // ذخیره تغییرات
        walletRepository.Update(wallet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // بازگشت نتیجه
        return new SettleCreditResponse(
            transaction.Id,
            wallet.Id,
            amountToSettle,
            DateTime.UtcNow
        );
    }
}