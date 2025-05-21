using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Application.Common.Contracts;

namespace Application.Transactions.Commands.DepositToWallet;
public class DepositToWalletCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DepositToWalletCommand, DepositToWalletResponse>
{
    public async Task<DepositToWalletResponse> Handle(DepositToWalletCommand request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        if (!wallet.IsActive)
            throw new BadRequestException("کیف پول غیرفعال است و امکان شارژ وجود ندارد");

        // پیدا کردن حساب متناسب با ارز درخواستی
        var account = wallet.Accounts.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
        if (account == null)
        {
            // اگر حساب برای ارز مورد نظر وجود نداشت، یک حساب جدید ایجاد می‌کنیم
            string accountNumber = GenerateAccountNumber();
            account = wallet.CreateAccount(request.Currency, accountNumber);
        }

        // انجام عملیات شارژ
        var transaction = account.Deposit(request.Amount, request.Description, request.PaymentReferenceId);

        // ذخیره تغییرات
        walletRepository.Update(wallet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // بازگشت نتیجه
        return new DepositToWalletResponse(
            transaction.Id,
            wallet.Id,
            account.Id,
            request.Amount,
            account.Balance,
            transaction.TransactionDate
        );
    }
 
}