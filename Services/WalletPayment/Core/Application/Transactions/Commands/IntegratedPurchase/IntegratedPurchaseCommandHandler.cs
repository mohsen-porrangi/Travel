using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.IntegratedPurchase;
public class IntegratedPurchaseCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork,
    ICurrencyExchangeService currencyExchangeService)
    : ICommandHandler<IntegratedPurchaseCommand, ExecuteIntegratedPurchaseResponse> // تغییر نام
{
    public async Task<ExecuteIntegratedPurchaseResponse> Handle( // تغییر نام
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

            // پیدا کردن حساب متناسب با ارز درخواستی یا ایجاد حساب جدید
            var account = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
            if (account == null)
            {                
                account = wallet.CreateCurrencyAccount(request.Currency);
            }

            // بررسی موجودی حساب و تبدیل ارز در صورت نیاز
            decimal requiredAmount = request.Amount;
            bool needsCurrencyConversion = false;
            Guid? sourceAccountId = null;
            decimal convertedAmount = 0;
            decimal conversionFee = 0;

            if (account.Balance < requiredAmount && request.AutoConvertCurrency)
            {
                // بررسی سایر حساب‌ها برای موجودی کافی
                var otherAccounts = wallet.CurrencyAccount
                    .Where(a => a.IsActive && a.Balance > 0 && a.Currency != request.Currency)
                    .OrderByDescending(a => a.Balance) // اولویت با حساب‌های با موجودی بیشتر
                    .ToList();

                foreach (var sourceAccount in otherAccounts)
                {
                    // محاسبه مقدار مورد نیاز در ارز حساب منبع
                    var (requiredSourceAmount, fee) = await currencyExchangeService.CalculateConversionAsync(
                        requiredAmount - account.Balance, // مقدار کمبود
                        request.Currency,
                        sourceAccount.Currency);

                    // اگر موجودی کافی است، تبدیل ارز انجام می‌شود
                    if (sourceAccount.Balance >= requiredSourceAmount)
                    {
                        // ذخیره اطلاعات برای تبدیل ارز
                        needsCurrencyConversion = true;
                        sourceAccountId = sourceAccount.Id;
                        convertedAmount = requiredSourceAmount;
                        conversionFee = fee;
                        break;
                    }
                }
            }

            // شروع تراکنش
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // متغیرها برای نگهداری تراکنش‌های ایجاد شده
                var conversionSourceTransaction = default(WalletPayment.Domain.Entities.Transaction.Transaction);
                var conversionTargetTransaction = default(WalletPayment.Domain.Entities.Transaction.Transaction);

                // اگر نیاز به تبدیل ارز باشد، ابتدا تبدیل انجام می‌شود
                if (needsCurrencyConversion && sourceAccountId.HasValue)
                {
                    var sourceAccount = wallet.CurrencyAccount.First(a => a.Id == sourceAccountId.Value);

                    // برداشت از حساب منبع
                    var withdrawDescription = $"تبدیل ارز از {sourceAccount.Currency} به {request.Currency} برای خرید {request.Description}";
                    conversionSourceTransaction = sourceAccount.Withdraw(
                        convertedAmount,
                        TransactionType.Transfer,
                        withdrawDescription);

                    // محاسبه مقدار نهایی پس از تبدیل (با کسر کارمزد)
                    var finalConvertedAmount = convertedAmount - conversionFee;

                    // واریز به حساب هدف
                    var depositDescription = $"دریافت از تبدیل ارز {sourceAccount.Currency} برای خرید {request.Description}";
                    conversionTargetTransaction = account.Deposit(
                        finalConvertedAmount,
                        depositDescription,
                        null);
                }

                // 1. شارژ کیف پول
                var depositTransaction = account.Deposit(
                    request.Amount,
                    $"شارژ خودکار برای سفارش {request.OrderId}",
                    request.PaymentReferenceId);

                // 2. برداشت از کیف پول
                var purchaseTransaction = account.Withdraw(
                    request.Amount,
                    TransactionType.Purchase,
                    request.Description,
                    request.OrderId);

                // ذخیره تغییرات و تأیید تراکنش
                walletRepository.Update(wallet);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                // بازگشت نتیجه
                return new ExecuteIntegratedPurchaseResponse(
                    depositTransaction.Id,
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
        catch (Exception ex)
        {
            throw;
        }
    }

}