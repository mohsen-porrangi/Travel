using BuildingBlocks.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Application.Transactions.Commands.ProcessTransaction;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Payment.Commands.CreateIntegratedPurchase;

public class CreateIntegratedPurchaseCommandHandler(
    IWalletRepository walletRepository,
    IIntegratedPurchaseService integratedPurchaseService,
    ILogger<CreateIntegratedPurchaseCommandHandler> logger,
    ISender mediator) : ICommandHandler<CreateIntegratedPurchaseCommand, CreateIntegratedPurchaseResponse>

{
    public async Task<CreateIntegratedPurchaseResponse> Handle(
        CreateIntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // بررسی موجودی فعلی کیف پول
            var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (wallet == null)
            {
                return new CreateIntegratedPurchaseResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "کیف پول کاربر یافت نشد"
                };
            }

            // یافتن حساب با ارز مورد نظر
            var account = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
            decimal currentBalance = account?.Balance ?? 0;

            // تصمیم‌گیری بر اساس موجودی
            if (currentBalance >= request.Amount)
            {
                // موجودی کافی است - برداشت مستقیم از کیف پول بدون درگاه
                logger.LogInformation(
                    "موجودی کیف پول کافی است. برداشت مستقیم برای سفارش {OrderId} به مبلغ {Amount}",
                    request.OrderId, request.Amount);

                // اجرای برداشت مستقیم
                var withdrawCommand = new ProcessWalletTransactionCommand(
                    request.UserId,
                    request.Amount,
                    request.Currency,
                    TransactionDirection.Out,
                    null,
                    request.OrderId,
                    request.Description);

                var withdrawResult = await mediator.Send(withdrawCommand, cancellationToken);

                return new CreateIntegratedPurchaseResponse
                {
                    IsSuccessful = true,
                    RequiresPayment = false,
                    CurrentBalance = currentBalance,
                    AmountFromWallet = request.Amount,
                    AmountToPay = 0,
                    TotalAmount = request.Amount,
                    TransactionId = withdrawResult.TransactionId
                };
            }
            else
            {
                // موجودی کافی نیست - نیاز به شارژ از درگاه
                decimal amountToPay = request.Amount - currentBalance;

                logger.LogInformation(
                    "موجودی کیف پول ناکافی. موجودی: {Balance}, نیاز به پرداخت: {AmountToPay}",
                    currentBalance, amountToPay);

                // اضافه کردن اطلاعات به متادیتا
                var metadata = request.Metadata ?? new Dictionary<string, string>();
                metadata["IntegratedPurchase"] = "true";
                metadata["CurrentBalance"] = currentBalance.ToString();
                metadata["AmountFromWallet"] = currentBalance.ToString();
                metadata["AmountFromGateway"] = amountToPay.ToString();
                metadata["TotalAmount"] = request.Amount.ToString();

                // ایجاد درخواست پرداخت فقط برای مابه‌التفاوت
                var result = await integratedPurchaseService.CreateIntegratedPurchaseRequestAsync(
                    request.UserId,
                    amountToPay, // فقط مابه‌التفاوت
                    request.Currency,
                    request.Description,
                    request.GatewayType,
                    request.CallbackUrl,
                    request.OrderId,
                    cancellationToken);

                return new CreateIntegratedPurchaseResponse
                {
                    IsSuccessful = result.IsSuccessful,
                    PaymentUrl = result.PaymentUrl,
                    Authority = result.Authority,
                    CurrentBalance = currentBalance,
                    AmountFromWallet = currentBalance,
                    AmountToPay = amountToPay,
                    TotalAmount = request.Amount,
                    RequiresPayment = true,
                    ErrorMessage = result.ErrorMessage
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در پردازش خرید یکپارچه");
            return new CreateIntegratedPurchaseResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"خطای سیستمی: {ex.Message}"
            };
        }
    }
}