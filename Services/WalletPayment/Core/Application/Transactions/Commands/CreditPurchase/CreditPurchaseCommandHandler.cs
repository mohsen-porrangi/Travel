using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.CreditPurchase;

public class CreditPurchaseCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreditPurchaseCommand, CreditPurchaseResponse>
{
    public async Task<CreditPurchaseResponse> Handle(CreditPurchaseCommand request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        if (!wallet.IsActive)
            throw new BadRequestException("کیف پول غیرفعال است و امکان خرید وجود ندارد");

        // بررسی اعتبار
        wallet.CheckCreditDueDate(); // بررسی سررسید اعتبار

        if (wallet.CreditBalance <= 0 || !wallet.CreditDueDate.HasValue)
            throw new BadRequestException("کاربر دارای اعتبار فعال نیست");

        if (wallet.CreditBalance < request.Amount)
            throw new BadRequestException($"اعتبار کافی نیست. اعتبار موجود: {wallet.CreditBalance}، مبلغ درخواستی: {request.Amount}");

        // پیدا کردن حساب متناسب با ارز درخواستی یا ایجاد حساب جدید
        var currencyAccount = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
        if (currencyAccount == null)
        {            
            currencyAccount = wallet.CreateAccount(request.Currency);
        }

        // استفاده از اعتبار
        bool usedCredit = wallet.UseCredit(request.Amount);
        if (!usedCredit)
            throw new BadRequestException("خطا در استفاده از اعتبار");

        // ایجاد تراکنش
        var transaction = new Domain.Entities.Transaction.Transaction(
            currencyAccount.Id,
            wallet.Id,
            request.Amount,
            TransactionDirection.Out,
            Domain.Entities.Enums.TransactionType.Purchase,
            Domain.Entities.Enums.TransactionStatus.Completed,
            request.Currency,
            request.Description,
            true, // این یک تراکنش اعتباری است
            wallet.CreditDueDate,
            null,
            request.OrderId
        );

        // ذخیره تغییرات
        walletRepository.Update(wallet);
        await walletRepository.AddTransactionAsync(transaction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // بازگشت نتیجه
        return new CreditPurchaseResponse(
            transaction.Id,
            wallet.Id,
            request.Amount,
            wallet.CreditBalance,
            transaction.TransactionDate
        );
    }
 
}