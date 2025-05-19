using Application.Transactions.Commands.WithdrawFromWallet;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Common.Exceptions;
using WalletPayment.Domain.Entities.Enums;


namespace WalletPayment.Application.Transactions.Commands.WithdrawFromWallet;

public class WithdrawFromWalletCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<WithdrawFromWalletCommand, WithdrawFromWalletResponse>
{
    public async Task<WithdrawFromWalletResponse> Handle(WithdrawFromWalletCommand request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        if (!wallet.IsActive)
            throw new BadRequestException("کیف پول غیرفعال است و امکان برداشت وجود ندارد");

        // پیدا کردن حساب متناسب با ارز درخواستی
        var account = wallet.Accounts.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
        if (account == null)
            throw new NotFoundException($"حساب با ارز {request.Currency} برای کاربر یافت نشد", request.UserId);

        try
        {
            // انجام عملیات برداشت
            var transaction = account.Withdraw(
                request.Amount,
                TransactionType.Purchase,
                request.Description,
                request.OrderId);

            // ذخیره تغییرات
            walletRepository.Update(wallet);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // بازگشت نتیجه
            return new WithdrawFromWalletResponse(
                transaction.Id,
                wallet.Id,
                account.Id,
                request.Amount,
                account.Balance,
                transaction.TransactionDate
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("موجودی کافی نیست"))
        {
            throw new InsufficientBalanceException(wallet.Id, request.Amount, account.Balance);
        }
    }
}