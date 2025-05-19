using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.IntegratedPurchase;

public class IntegratedPurchaseCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<IntegratedPurchaseCommand, IntegratedPurchaseResponse>
{
    public async Task<IntegratedPurchaseResponse> Handle(IntegratedPurchaseCommand request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        if (!wallet.IsActive)
            throw new BadRequestException("کیف پول غیرفعال است و امکان خرید وجود ندارد");

        // پیدا کردن حساب متناسب با ارز درخواستی یا ایجاد حساب جدید
        var account = wallet.Accounts.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
        if (account == null)
        {
            string accountNumber = GenerateAccountNumber();
            account = wallet.CreateAccount(request.Currency, accountNumber);
        }

        // شروع تراکنش
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. شارژ کیف پول
            var depositTransaction = account.Deposit(
                request.Amount,
                $"شارژ خودکار برای سفارش {request.OrderId}",
                request.PaymentReferenceId);

            // 2. برداشت از کیف پول
            var withdrawTransaction = account.Withdraw(
                request.Amount,
                TransactionType.Purchase,
                request.Description,
                request.OrderId);

            // ذخیره تغییرات و تأیید تراکنش
            walletRepository.Update(wallet);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // بازگشت نتیجه
            return new IntegratedPurchaseResponse(
                depositTransaction.Id,
                withdrawTransaction.Id,
                request.Amount,
                account.Balance,
                withdrawTransaction.TransactionDate
            );
        }
        catch (Exception)
        {
            // در صورت بروز خطا، برگشت تراکنش
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private string GenerateAccountNumber()
    {
        // ساخت شماره حساب تصادفی 16 رقمی
        return $"6037{new Random().Next(100000000, 999999999).ToString().PadRight(12, '0')}";
    }
}