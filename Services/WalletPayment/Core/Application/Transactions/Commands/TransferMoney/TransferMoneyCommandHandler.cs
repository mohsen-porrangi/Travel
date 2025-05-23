using BuildingBlocks.Contracts.Services;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Common.Exceptions;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.Transactions.Commands.TransferMoney;

public class TransferMoneyCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork,
    ITransferFeeCalculator feeCalculator)
    : ICommandHandler<TransferMoneyCommand, TransferMoneyResponse>
{
    public async Task<TransferMoneyResponse> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
    {
        // بررسی کیف پول مبدأ
        var sourceWallet = await walletRepository.GetByUserIdAsync(request.SourceUserId, cancellationToken);
        if (sourceWallet == null)
            throw new NotFoundException("کیف پول مبدأ یافت نشد", request.SourceUserId);

        if (!sourceWallet.IsActive)
            throw new BadRequestException("کیف پول مبدأ غیرفعال است");

        // بررسی کیف پول مقصد
        var targetWallet = await walletRepository.GetByUserIdAsync(request.TargetUserId, cancellationToken);
        if (targetWallet == null)
            throw new NotFoundException("کیف پول مقصد یافت نشد", request.TargetUserId);

        if (!targetWallet.IsActive)
            throw new BadRequestException("کیف پول مقصد غیرفعال است");

        // پیدا کردن/ایجاد حساب مبدأ
        var sourceAccount = sourceWallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
        if (sourceAccount == null)
            throw new BadRequestException($"حساب مبدأ با ارز {request.Currency} یافت نشد");

        // پیدا کردن/ایجاد حساب مقصد
        var targetAccount = targetWallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
        if (targetAccount == null)
        {
            // ایجاد خودکار حساب با ارز مناسب در مقصد           
            targetAccount = targetWallet.CreateCurrencyAccount(request.Currency);
        }

        // محاسبه کارمزد انتقال
        decimal feeAmount = feeCalculator.CalculateTransferFee(request.Amount, request.Currency);
        decimal totalDeduction = request.Amount + feeAmount;

        // بررسی موجودی کافی
        if (sourceAccount.Balance < totalDeduction)
            throw new InsufficientBalanceException(sourceWallet.Id, totalDeduction, sourceAccount.Balance);

        // ایجاد یک شناسه منحصر به فرد برای انتقال
        var transferId = Guid.NewGuid();

        // شروع تراکنش
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // انتشار رویداد شروع انتقال
            sourceWallet.AddDomainEvent(new TransferInitiatedEvent(
                sourceWallet.Id,
                sourceAccount.Id,
                targetWallet.Id,
                targetAccount.Id,
                request.Amount,
                request.Currency,
                request.Description));

            // برداشت از حساب مبدأ (شامل کارمزد)
            var withdrawDescription = $"انتقال به کاربر دیگر - {request.Description}";
            var sourceTransaction = sourceAccount.Withdraw(
                totalDeduction,
                Domain.Entities.Enums.TransactionType.Transfer,
                withdrawDescription);

            sourceTransaction.LinkToTransfer(transferId);

            // واریز به حساب مقصد
            var depositDescription = $"دریافت از کاربر دیگر - {request.Description}";
            var targetTransaction = targetAccount.Deposit(
                request.Amount,
                depositDescription,
                transferId.ToString());

            targetTransaction.LinkToTransfer(transferId);

            // ذخیره تغییرات
            walletRepository.Update(sourceWallet);
            walletRepository.Update(targetWallet);

            // انتشار رویداد تکمیل انتقال
            sourceWallet.AddDomainEvent(new TransferCompletedEvent(
                transferId,
                sourceWallet.Id,
                targetWallet.Id,
                request.Amount,
                request.Currency,
                feeAmount));

            // تأیید تراکنش
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // بازگشت نتیجه
            return new TransferMoneyResponse
            {
                TransferId = transferId,
                SourceTransactionId = sourceTransaction.Id,
                TargetTransactionId = targetTransaction.Id,
                TransferredAmount = request.Amount,
                FeeAmount = feeAmount,
                SourceRemainingBalance = sourceAccount.Balance,
                TargetNewBalance = targetAccount.Balance,
                TransferDate = DateTime.UtcNow
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