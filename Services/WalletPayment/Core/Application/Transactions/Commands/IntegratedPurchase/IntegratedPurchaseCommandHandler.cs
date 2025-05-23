using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Application.Common.Exceptions;

namespace WalletPayment.Application.Transactions.Commands.IntegratedPurchase;

public class IntegratedPurchaseCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork,
    ICurrencyExchangeService currencyExchangeService)
    : ICommandHandler<IntegratedPurchaseCommand, ExecuteIntegratedPurchaseResponse>
{
    public async Task<ExecuteIntegratedPurchaseResponse> Handle(
        IntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // دریافت کیف پول کاربر
            var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (wallet == null)
                throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

            if (!wallet.IsActive)
                throw new BadRequestException("کیف پول غیرفعال است و امکان خرید وجود ندارد");

            // پیدا کردن حساب متناسب با ارز درخواستی
            var account = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
            if (account == null)
            {
                account = wallet.CreateCurrencyAccount(request.Currency);
            }

            // بررسی موجودی کافی
            if (account.Balance < request.Amount)
            {
                throw new InsufficientBalanceException(
                    wallet.Id,
                    request.Amount,
                    account.Balance);
            }

            // شروع تراکنش
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Note: در اینجا فرض می‌کنیم که:
                // 1. اگر PaymentReferenceId دارد، یعنی شارژ از درگاه انجام شده
                // 2. موجودی کیف پول به‌روز است و شامل مبلغ شارژ شده است

                // ایجاد یک شناسه یکتا برای خرید
                var purchaseId = Guid.NewGuid();

                // برداشت از حساب برای خرید
                var purchaseTransaction = account.Withdraw(
                    request.Amount,
                    TransactionType.Purchase,
                    request.Description,
                    request.OrderId);

                // اگر شماره مرجع پرداخت داریم، آن را ثبت کنیم
                if (!string.IsNullOrEmpty(request.PaymentReferenceId))
                {
                    // این نشان می‌دهد که بخشی از مبلغ از طریق درگاه پرداخت شده
                    purchaseTransaction.SetRelatedTransactionId(purchaseId);
                }

                // ذخیره تغییرات و تأیید تراکنش
                walletRepository.Update(wallet);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                // بازگشت نتیجه
                return new ExecuteIntegratedPurchaseResponse(
                    Guid.Empty, // در این روش جدید، deposit transaction id نداریم
                    purchaseTransaction.Id,
                    request.Amount,
                    account.Balance,
                    purchaseTransaction.TransactionDate
                );
            }
            catch (Exception)
            {
                // در صورت بروز خطا، برگشت تراکنش
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (InsufficientBalanceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException(
                "خطا در انجام خرید یکپارچه",
                $"UserId: {request.UserId}, Amount: {request.Amount}, OrderId: {request.OrderId}");
        }
    }
}