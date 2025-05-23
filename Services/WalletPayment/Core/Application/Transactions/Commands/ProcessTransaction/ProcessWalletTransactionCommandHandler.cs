using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Common.Exceptions;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.ProcessTransaction;

public class ProcessWalletTransactionCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ProcessWalletTransactionCommand, WalletTransactionResponse>
{
    public async Task<WalletTransactionResponse> Handle(ProcessWalletTransactionCommand request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        if (!wallet.IsActive)
            throw new BadRequestException("کیف پول غیرفعال است و امکان انجام تراکنش وجود ندارد");

        // پیدا کردن حساب متناسب با ارز درخواستی یا ایجاد حساب جدید
        var currencyAccount = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
        if (currencyAccount == null)
        {
            if (request.Direction == TransactionDirection.Out)
                throw new NotFoundException($"حساب با ارز {request.Currency} برای کاربر یافت نشد", request.UserId);

            // برای واریز، حساب جدید ایجاد می‌کنیم
            string accountNumber = GenerateAccountNumber();
            currencyAccount = wallet.CreateCurrencyAccount(request.Currency);
        }

        try
        {
            // انجام تراکنش بر اساس جهت (واریز یا برداشت)
            WalletPayment.Domain.Entities.Transaction.Transaction transaction;

            if (request.Direction == TransactionDirection.In)
            {
                // واریز به حساب
                transaction = currencyAccount.Deposit(
                    request.Amount,
                    request.Description,
                    request.ReferenceId);
            }
            else
            {
                // برداشت از حساب
                transaction = currencyAccount.Withdraw(
                    request.Amount,
                    TransactionType.Purchase,
                    request.Description,
                    request.OrderId);
            }

            // ذخیره تغییرات
            walletRepository.Update(wallet);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // بازگشت نتیجه
            return new WalletTransactionResponse
            {
                TransactionId = transaction.Id,
                WalletId = wallet.Id,
                AccountId = currencyAccount.Id,
                Amount = request.Amount,
                NewBalance = currencyAccount.Balance,
                Direction = request.Direction,
                TransactionDate = transaction.TransactionDate
            };
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("موجودی کافی نیست"))
        {
            throw new InsufficientBalanceException(wallet.Id, request.Amount, currencyAccount.Balance);
        }
    }

    private static string GenerateAccountNumber()
    {
        // ساخت شماره حساب تصادفی 16 رقمی
        return $"6037{new Random().Next(100000000, 999999999).ToString().PadRight(12, '0')}";
    }
}