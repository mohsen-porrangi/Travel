using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Common.Exceptions;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Domain.Events;

namespace Application.Transactions.Commands.ConvertCurrency;

public class ConvertCurrencyCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork,
    ICurrencyExchangeService currencyExchangeService)
    : ICommandHandler<ConvertCurrencyCommand, ConvertCurrencyResponse>
{
    public async Task<ConvertCurrencyResponse> Handle(ConvertCurrencyCommand request, CancellationToken cancellationToken)
    {
        // بررسی کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول یافت نشد", request.UserId);

        if (!wallet.IsActive)
            throw new BadRequestException("کیف پول غیرفعال است");

        // بررسی حساب مبدأ
        var sourceAccount = wallet.Accounts
            .FirstOrDefault(a => a.Currency == request.SourceCurrency && a.IsActive);

        if (sourceAccount == null)
            throw new NotFoundException(
                $"حساب با ارز {request.SourceCurrency} یافت نشد", request.UserId);

        // بررسی موجودی کافی
        if (sourceAccount.Balance < request.SourceAmount)
            throw new InsufficientBalanceException(
                wallet.Id, request.SourceAmount, sourceAccount.Balance);

        // بررسی/ایجاد حساب مقصد
        var targetAccount = wallet.Accounts
            .FirstOrDefault(a => a.Currency == request.TargetCurrency && a.IsActive);

        if (targetAccount == null)
        {
            // ایجاد خودکار حساب مقصد
            string accountNumber = GenerateAccountNumber();
            targetAccount = wallet.CreateAccount(request.TargetCurrency, accountNumber);
        }

        // دریافت نرخ تبدیل
        var exchangeRate = await currencyExchangeService.GetExchangeRateAsync(
            request.SourceCurrency, request.TargetCurrency);

        // محاسبه مبلغ نهایی و کارمزد
        var (targetAmount, feeAmount) = await currencyExchangeService.CalculateConversionAsync(
            request.SourceAmount, request.SourceCurrency, request.TargetCurrency);

        // ساخت شناسه منحصر به فرد برای تبدیل
        var conversionId = Guid.NewGuid();

        // انتشار رویداد درخواست تبدیل
        wallet.AddDomainEvent(new CurrencyConversionRequestedEvent(
            wallet.Id,
            sourceAccount.Id,
            targetAccount.Id,
            request.SourceAmount,
            request.SourceCurrency,
            request.TargetCurrency,
            targetAmount,
            exchangeRate));

        // شروع تراکنش
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // برداشت از حساب مبدأ
            var withdrawalDescription = $"تبدیل ارز از {request.SourceCurrency} به {request.TargetCurrency}";
            var sourceTransaction = sourceAccount.Withdraw(
                request.SourceAmount,
                TransactionType.Transfer,
                withdrawalDescription,
                conversionId.ToString());

            // ثبت ارتباط با تبدیل ارز
            sourceTransaction.SetRelatedTransactionId(conversionId);

            // واریز به حساب مقصد
            var depositDescription = $"تبدیل ارز از {request.SourceCurrency} به {request.TargetCurrency}";
            var targetTransaction = targetAccount.Deposit(
                targetAmount,
                depositDescription,
                conversionId.ToString());

            // ثبت ارتباط با تبدیل ارز
            targetTransaction.SetRelatedTransactionId(conversionId);

            // انتشار رویداد تکمیل تبدیل
            wallet.AddDomainEvent(new CurrencyConversionCompletedEvent(
                wallet.Id,
                sourceAccount.Id,
                targetAccount.Id,
                request.SourceAmount,
                targetAmount,
                request.SourceCurrency,
                request.TargetCurrency,
                exchangeRate,
                feeAmount));

            // ذخیره تغییرات
            walletRepository.Update(wallet);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // بازگشت نتیجه
            return new ConvertCurrencyResponse
            {
                ConversionId = conversionId,
                SourceTransactionId = sourceTransaction.Id,
                TargetTransactionId = targetTransaction.Id,
                SourceAmount = request.SourceAmount,
                TargetAmount = targetAmount,
                SourceCurrency = request.SourceCurrency,
                TargetCurrency = request.TargetCurrency,
                ExchangeRate = exchangeRate,
                FeeAmount = feeAmount,
                ConversionDate = DateTime.UtcNow
            };
        }
        catch
        {
            // برگشت تراکنش در صورت خطا
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}