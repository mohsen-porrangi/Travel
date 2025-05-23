using Infrastructure.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Application.Transactions.Commands.ProcessTransaction;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.Services;

/// <summary>
/// سرویس یکپارچه‌سازی عملیات پرداخت و کیف پول
/// این سرویس عملیات شارژ خودکار کیف پول و برداشت برای خرید را مدیریت می‌کند
/// </summary>
public class IntegratedPurchaseService(
       IWalletDbContext dbContext,
       IWalletRepository walletRepository,
       IPaymentService paymentService,
       IUnitOfWork unitOfWork,
       ISender mediator,
       ILogger<IntegratedPurchaseService> logger) : IIntegratedPurchaseService
{



    /// <summary>
    /// ایجاد درخواست پرداخت یکپارچه (فقط برای مابه‌التفاوت)
    /// </summary>
    public async Task<IntegratedPurchaseResult> CreateIntegratedPurchaseRequestAsync(
        Guid userId,
        decimal amount, // این مبلغ همان مابه‌التفاوت است
        CurrencyCode currency,
        string description,
        PaymentGatewayType gatewayType,
        string callbackUrl,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "شروع فرآیند شارژ کیف پول برای خرید یکپارچه. کاربر {UserId}، مبلغ مابه‌التفاوت {Amount}، شناسه سفارش {OrderId}",
                userId, amount, orderId);

            // بررسی وجود کیف پول
            var wallet = await walletRepository.GetByUserIdAsync(userId, cancellationToken);
            if (wallet == null)
            {
                logger.LogWarning("کیف پول برای کاربر {UserId} یافت نشد", userId);
                return new IntegratedPurchaseResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "کیف پول برای کاربر مورد نظر یافت نشد"
                };
            }

            // افزودن متادیتا برای شناسایی خرید یکپارچه
            var metadata = new Dictionary<string, string>
            {
                ["IntegratedPurchase"] = "true",
                ["OrderId"] = orderId,
                ["Description"] = description,
                ["PaymentAmount"] = amount.ToString(), // مبلغ پرداختی از درگاه
            };

            // ایجاد درخواست پرداخت فقط برای مابه‌التفاوت
            var paymentResult = await paymentService.CreatePaymentRequestAsync(
                userId,
                amount, // فقط مابه‌التفاوت
                currency,
                $"شارژ کیف پول برای تکمیل خرید {description} - سفارش {orderId}",
                gatewayType,
                callbackUrl,
                metadata,
                orderId,
                null,
                cancellationToken);

            if (!paymentResult.IsSuccessful)
            {
                logger.LogError(
                    "خطا در ایجاد درخواست پرداخت برای مابه‌التفاوت: {ErrorMessage}",
                    paymentResult.ErrorMessage);

                return new IntegratedPurchaseResult
                {
                    IsSuccessful = false,
                    ErrorMessage = paymentResult.ErrorMessage
                };
            }

            logger.LogInformation(
                "درخواست پرداخت مابه‌التفاوت با موفقیت ایجاد شد. شناسه مرجع: {Authority}, مبلغ: {Amount}",
                paymentResult.Authority, amount);

            return new IntegratedPurchaseResult
            {
                IsSuccessful = true,
                PaymentUrl = paymentResult.PaymentUrl,
                Authority = paymentResult.Authority
            };
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "خطا در ایجاد درخواست پرداخت مابه‌التفاوت: کاربر {UserId}, مبلغ {Amount}",
                userId, amount);

            return new IntegratedPurchaseResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطای داخلی در ایجاد درخواست پرداخت"
            };
        }
    }

    /// <summary>
    /// انجام عملیات خرید یکپارچه پس از تأیید پرداخت
    /// </summary>
    public async Task<IntegratedPurchaseCompletionResult> CompleteIntegratedPurchaseAsync(
        Guid userId,
        decimal totalAmount, // مبلغ کل خرید
        CurrencyCode currency,
        string orderId,
        string paymentReferenceId,
        string description,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "تکمیل فرآیند خرید یکپارچه برای کاربر {UserId}، مبلغ کل {TotalAmount}، شناسه سفارش {OrderId}",
                userId, totalAmount, orderId);

            // دریافت کیف پول
            var wallet = await walletRepository.GetByUserIdAsync(userId, cancellationToken);
            if (wallet == null)
            {
                return new IntegratedPurchaseCompletionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "کیف پول یافت نشد"
                };
            }

            // یافتن حساب با ارز مورد نظر
            var account = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == currency && a.IsActive);
            if (account == null)
            {
                return new IntegratedPurchaseCompletionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "حساب ارزی یافت نشد"
                };
            }

            // شروع تراکنش
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Note: شارژ کیف پول قبلاً در PaymentService انجام شده است
                // در اینجا فقط باید برداشت کل مبلغ خرید را انجام دهیم

                // برداشت کل مبلغ خرید از کیف پول
                var withdrawCommand = new ProcessWalletTransactionCommand(
                    userId,
                    totalAmount,
                    currency,
                    TransactionDirection.Out,
                    null,
                    orderId,
                    description);

                var withdrawResult = await mediator.Send(withdrawCommand, cancellationToken);

                // تأیید تراکنش
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                logger.LogInformation(
                    "فرآیند خرید یکپارچه با موفقیت تکمیل شد. شناسه تراکنش برداشت: {WithdrawTransactionId}",
                    withdrawResult.TransactionId);

                return new IntegratedPurchaseCompletionResult
                {
                    IsSuccessful = true,
                    DepositTransactionId = Guid.Empty, // شارژ قبلاً انجام شده
                    WithdrawTransactionId = withdrawResult.TransactionId,
                    Amount = totalAmount,
                    RemainingBalance = withdrawResult.NewBalance
                };
            }
            catch (Exception ex)
            {
                // برگرداندن تراکنش در صورت بروز خطا
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "خطا در تکمیل خرید یکپارچه: کاربر {UserId}, مبلغ {Amount}, سفارش {OrderId}",
                userId, totalAmount, orderId);

            return new IntegratedPurchaseCompletionResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطای داخلی در تکمیل فرآیند خرید"
            };
        }
    }
}