using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Domain.Entities.Enums;
using MediatR;
using WalletPayment.Application.Transactions.Commands.ProcessTransaction;

namespace WalletPayment.Infrastructure.Services;

/// <summary>
/// سرویس یکپارچه‌سازی عملیات پرداخت و کیف پول
/// این سرویس عملیات شارژ خودکار کیف پول و برداشت برای خرید را مدیریت می‌کند
/// </summary>
public class IntegratedPurchaseService : IIntegratedPurchaseService
{
    private readonly IWalletDbContext _dbContext;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _mediator;
    private readonly ILogger<IntegratedPurchaseService> _logger;

    public IntegratedPurchaseService(
        IWalletDbContext dbContext,
        IWalletRepository walletRepository,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ISender mediator,
        ILogger<IntegratedPurchaseService> logger)
    {
        _dbContext = dbContext;
        _walletRepository = walletRepository;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// ایجاد درخواست پرداخت یکپارچه (شارژ کیف پول و برداشت همزمان)
    /// </summary>
    public async Task<IntegratedPurchaseResult> CreateIntegratedPurchaseRequestAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string description,
        PaymentGatewayType gatewayType,
        string callbackUrl,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "شروع فرآیند خرید یکپارچه برای کاربر {UserId}، مبلغ {Amount}، شناسه سفارش {OrderId}",
                userId, amount, orderId);

            // بررسی وجود کیف پول
            var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("کیف پول برای کاربر {UserId} یافت نشد", userId);
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
                ["Description"] = description
            };

            // ایجاد درخواست پرداخت
            var paymentResult = await _paymentService.CreatePaymentRequestAsync(
                userId,
                amount,
                currency,
                $"شارژ کیف پول برای خرید {description} - سفارش {orderId}",
                gatewayType,
                callbackUrl,
                metadata,
                orderId,
                null,
                cancellationToken);

            if (!paymentResult.IsSuccessful)
            {
                _logger.LogError(
                    "خطا در ایجاد درخواست پرداخت یکپارچه: {ErrorMessage}",
                    paymentResult.ErrorMessage);

                return new IntegratedPurchaseResult
                {
                    IsSuccessful = false,
                    ErrorMessage = paymentResult.ErrorMessage
                };
            }

            _logger.LogInformation(
                "درخواست پرداخت یکپارچه با موفقیت ایجاد شد. شناسه مرجع: {Authority}",
                paymentResult.Authority);

            return new IntegratedPurchaseResult
            {
                IsSuccessful = true,
                PaymentUrl = paymentResult.PaymentUrl,
                Authority = paymentResult.Authority
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا در ایجاد درخواست پرداخت یکپارچه: کاربر {UserId}, مبلغ {Amount}",
                userId, amount);

            return new IntegratedPurchaseResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطای داخلی در ایجاد درخواست پرداخت یکپارچه"
            };
        }
    }

    /// <summary>
    /// انجام عملیات خرید یکپارچه پس از تأیید پرداخت
    /// </summary>
    public async Task<IntegratedPurchaseCompletionResult> CompleteIntegratedPurchaseAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string orderId,
        string paymentReferenceId,
        string description,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "تکمیل فرآیند خرید یکپارچه برای کاربر {UserId}، مبلغ {Amount}، شناسه سفارش {OrderId}",
                userId, amount, orderId);

            // شروع تراکنش
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. شارژ کیف پول
                var depositCommand = new ProcessWalletTransactionCommand(
                    userId,
                    amount,
                    currency,
                    TransactionDirection.In,
                    paymentReferenceId,
                    null,
                    description);
                   //TODO $"شارژ خودکار برای سفارش {orderId} - {description}");

                var depositResult = await _mediator.Send(depositCommand, cancellationToken);

                // 2. برداشت از کیف پول برای خرید
                var withdrawCommand = new ProcessWalletTransactionCommand(
                     userId,
                     amount,
                     currency,
                     TransactionDirection.Out,
                     null,
                     orderId,
                     description);

                var withdrawResult = await _mediator.Send(withdrawCommand, cancellationToken);

                // تأیید تراکنش
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "فرآیند خرید یکپارچه با موفقیت تکمیل شد. شناسه تراکنش واریز: {DepositTransactionId}, " +
                    "شناسه تراکنش برداشت: {WithdrawTransactionId}",
                    depositResult.TransactionId, withdrawResult.TransactionId);

                return new IntegratedPurchaseCompletionResult
                {
                    IsSuccessful = true,
                    DepositTransactionId = depositResult.TransactionId,
                    WithdrawTransactionId = withdrawResult.TransactionId,
                    Amount = amount,
                    RemainingBalance = withdrawResult.RemainingBalance
                };
            }
            catch (Exception ex)
            {
                // برگرداندن تراکنش در صورت بروز خطا
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا در تکمیل خرید یکپارچه: کاربر {UserId}, مبلغ {Amount}, سفارش {OrderId}",
                userId, amount, orderId);

            return new IntegratedPurchaseCompletionResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطای داخلی در تکمیل فرآیند خرید یکپارچه"
            };
        }
    }
}