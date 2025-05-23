using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Infrastructure.ExternalServices.PaymentGateway;
using WalletPayment.Domain.Entities.Payment;

namespace WalletPayment.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IWalletDbContext _dbContext;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentTransactionService _paymentTransactionService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IWalletDbContext dbContext,
        IWalletRepository walletRepository,
        IPaymentGatewayFactory gatewayFactory,
        IUnitOfWork unitOfWork,
        IPaymentTransactionService paymentTransactionService,
        ILogger<PaymentService> logger)
    {
        _dbContext = dbContext;
        _walletRepository = walletRepository;
        _gatewayFactory = gatewayFactory;
        _unitOfWork = unitOfWork;
        _paymentTransactionService = paymentTransactionService;
        _logger = logger;
    }

    public async Task<PaymentRequestResult> CreatePaymentRequestAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string description,
        PaymentGatewayType gatewayType,
        string callbackUrl,
        Dictionary<string, string>? metadata = null,
        string? orderId = null,
        string? cancelUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی وجود کاربر و کیف پول
            var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("درخواست پرداخت برای کاربر بدون کیف پول: {UserId}", userId);
                return PaymentRequestResult.Failure("کیف پول برای کاربر مورد نظر یافت نشد", PaymentErrorCode.Unknown);
            }

            // ایجاد شی پرداخت در دیتابیس
            var payment = new Payment(
                wallet.Id,
                null, // فعلاً به حساب مشخصی متصل نمی‌شود
                amount,
                currency,
                gatewayType,
                description,
                callbackUrl
            );

            // ذخیره اطلاعات اضافی
            if (metadata != null)
            {
                var metadataJson = JsonSerializer.Serialize(metadata);
                payment.SetAdditionalData(metadataJson);
            }

            await _dbContext.Payments.AddAsync(payment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // ایجاد تراکنش پرداخت
            var transactionId = await _paymentTransactionService.CreatePaymentTransactionAsync(
                userId, amount, gatewayType, description, orderId, cancellationToken);

            // ایجاد درخواست پرداخت در درگاه
            var gateway = _gatewayFactory.CreateGateway(gatewayType);
            var request = new PaymentRequestDto(
                amount,
                currency,
                description,
                callbackUrl,
                "", // شماره موبایل (می‌تواند از اطلاعات کاربر دریافت شود)
                "", // ایمیل (می‌تواند از اطلاعات کاربر دریافت شود)
                userId,
                metadata ?? new Dictionary<string, string>()
            );

            var result = await gateway.CreatePaymentRequestAsync(request, cancellationToken);

            if (result.IsSuccessful)
            {
                // بروزرسانی رکورد پرداخت با شناسه درگاه
                payment.SetAuthority(result.Authority);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // بروزرسانی تراکنش پرداخت
                await _paymentTransactionService.UpdatePaymentTransactionStatusAsync(
                    transactionId,
                    Domain.Entities.Enums.PaymentTransactionStatus.Processing,
                    null,
                    $"Authority: {result.Authority}",
                    cancellationToken);

                _logger.LogInformation(
                    "درخواست پرداخت با موفقیت ایجاد شد. کاربر: {UserId}, مبلغ: {Amount}, درگاه: {Gateway}",
                    userId, amount, gatewayType);
            }
            else
            {
                _logger.LogWarning(
                    "خطا در ایجاد درخواست پرداخت. کاربر: {UserId}, درگاه: {Gateway}, خطا: {Error}",
                    userId, gatewayType, result.ErrorMessage);

                // بروزرسانی وضعیت پرداخت به ناموفق
                payment.MarkAsFailed(result.ErrorMessage, result.ErrorCode.Value);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // بروزرسانی تراکنش پرداخت
                await _paymentTransactionService.UpdatePaymentTransactionStatusAsync(
                    transactionId,
                    Domain.Entities.Enums.PaymentTransactionStatus.Failed,
                    null,
                    result.ErrorMessage,
                    cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ایجاد درخواست پرداخت. کاربر: {UserId}, مبلغ: {Amount}", userId, amount);
            return PaymentRequestResult.Failure("خطای داخلی در ایجاد درخواست پرداخت", PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(
        string authority,
        string status,
        decimal originalAmount,
        Guid userId,
        bool automaticDeposit = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // یافتن رکورد پرداخت با شناسه مربوطه
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Authority == authority, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("درخواست تأیید پرداخت برای تراکنش نامعتبر. شناسه: {Authority}", authority);
                return PaymentVerificationResult.Failure("پرداخت مورد نظر یافت نشد", PaymentErrorCode.AuthorityNotFound);
            }

            // یافتن تراکنش پرداخت مرتبط
            var paymentTransaction = await _paymentTransactionService.GetPaymentTransactionByTokenAsync(authority, cancellationToken);
            if (paymentTransaction == null)
            {
                _logger.LogWarning("تراکنش پرداخت برای شناسه {Authority} یافت نشد", authority);
                return PaymentVerificationResult.Failure("تراکنش پرداخت یافت نشد", PaymentErrorCode.AuthorityNotFound);
            }

            // ایجاد درخواست تأیید پرداخت
            var gateway = _gatewayFactory.CreateGateway(payment.GatewayType);
            var request = new PaymentVerificationRequestDto(
                authority,
                status,
                payment.Amount,
                new Dictionary<string, string>()
            );

            // ارسال درخواست تأیید به درگاه
            var result = await gateway.VerifyPaymentAsync(request, cancellationToken);

            if (result.IsSuccessful)
            {
                _logger.LogInformation(
                    "تأیید پرداخت با موفقیت انجام شد. شناسه: {Authority}, شناسه ارجاع: {ReferenceId}",
                    authority, result.ReferenceId);

                // بروزرسانی وضعیت پرداخت
                payment.MarkAsPaid(result.ReferenceId, DateTime.UtcNow);

                // اگر واریز خودکار فعال باشد
                if (automaticDeposit)
                {
                    // شارژ کیف پول
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    try
                    {
                        // یافتن کیف پول
                        var wallet = await _walletRepository.GetByIdAsync(payment.WalletId, cancellationToken);
                        if (wallet != null)
                        {
                            // پیدا کردن یا ایجاد حساب متناسب با ارز
                            var currencyAccount = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == payment.Currency && a.IsActive);
                            if (currencyAccount == null)
                            {
                                // ایجاد حساب جدید                                
                                currencyAccount = wallet.CreateCurrencyAccount(payment.Currency);
                            }

                            // شارژ حساب
                            var transaction = currencyAccount.Deposit(
                                payment.Amount,
                                $"شارژ مستقیم از درگاه {payment.GatewayType} - {payment.Description}",
                                result.ReferenceId);

                            // بروزرسانی وضعیت پرداخت به تأیید شده
                            payment.MarkAsVerified(transaction.Id);

                            // بروزرسانی کیف پول
                            _walletRepository.Update(wallet);

                            // بروزرسانی تراکنش پرداخت
                            await _paymentTransactionService.CompleteSuccessfulPaymentAsync(
                                paymentTransaction.Id, result.ReferenceId, cancellationToken);

                            await _unitOfWork.CommitTransactionAsync(cancellationToken);

                            _logger.LogInformation(
                                "شارژ کیف پول با موفقیت انجام شد. کاربر: {UserId}, مبلغ: {Amount}",
                                userId, payment.Amount);
                        }
                        else
                        {
                            _logger.LogWarning("کیف پول برای شارژ خودکار یافت نشد. شناسه: {WalletId}", payment.WalletId);
                            await _dbContext.SaveChangesAsync(cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        _logger.LogError(ex, "خطا در شارژ خودکار کیف پول. شناسه پرداخت: {PaymentId}", payment.Id);

                        // فقط وضعیت پرداخت را تأیید می‌کنیم بدون شارژ کیف پول
                        payment.MarkAsPaid(result.ReferenceId, DateTime.UtcNow);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
                else
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                _logger.LogWarning(
                    "تأیید پرداخت ناموفق بود. شناسه: {Authority}, خطا: {Error}",
                    authority, result.ErrorMessage);

                // بروزرسانی وضعیت پرداخت به ناموفق
                payment.MarkAsFailed(result.ErrorMessage, result.ErrorCode.Value);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // بروزرسانی تراکنش پرداخت
                await _paymentTransactionService.UpdatePaymentTransactionStatusAsync(
                    paymentTransaction.Id,
                    Domain.Entities.Enums.PaymentTransactionStatus.Failed,
                    null,
                    result.ErrorMessage,
                    cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در تأیید پرداخت. شناسه: {Authority}", authority);
            return PaymentVerificationResult.Failure("خطای داخلی در تأیید پرداخت", PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentRefundResult> RefundPaymentAsync(
        Guid paymentId,
        Guid userId,
        string reason,
        decimal? amount = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // یافتن رکورد پرداخت
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("درخواست استرداد برای پرداخت نامعتبر. شناسه: {PaymentId}", paymentId);
                return PaymentRefundResult.Failure("پرداخت مورد نظر یافت نشد", PaymentErrorCode.AuthorityNotFound);
            }

            // بررسی تطابق کاربر با مالک پرداخت
            var wallet = await _walletRepository.GetByIdAsync(payment.WalletId, cancellationToken);
            if (wallet == null || wallet.UserId != userId)
            {
                _logger.LogWarning("درخواست استرداد توسط کاربر غیرمجاز. پرداخت: {PaymentId}, کاربر: {UserId}", paymentId, userId);
                return PaymentRefundResult.Failure("شما مجاز به استرداد این پرداخت نیستید", PaymentErrorCode.Unknown);
            }

            // بررسی امکان استرداد
            // بررسی امکان استرداد
            if (payment.Status != PaymentStatus.Verified && payment.Status != PaymentStatus.Paid)
            {
                _logger.LogWarning("درخواست استرداد برای پرداخت غیرقابل استرداد. وضعیت: {Status}", payment.Status);
                return PaymentRefundResult.Failure("این پرداخت قابل استرداد نیست", PaymentErrorCode.RefundNotAllowed);
            }

            // محاسبه مبلغ استرداد
            var refundAmount = amount ?? payment.Amount;
            if (refundAmount > payment.Amount)
            {
                _logger.LogWarning("درخواست استرداد با مبلغ بیشتر از پرداخت اصلی. درخواست: {RefundAmount}, اصلی: {OriginalAmount}", refundAmount, payment.Amount);
                return PaymentRefundResult.Failure("مبلغ استرداد نمی‌تواند بیشتر از مبلغ پرداخت باشد", PaymentErrorCode.InvalidAmount);
            }

            // ارسال درخواست استرداد به درگاه
            var gateway = _gatewayFactory.CreateGateway(payment.GatewayType);
            var request = new PaymentRefundRequestDto(
                payment.ReferenceId,
                refundAmount,
                reason
            );

            var result = await gateway.RefundPaymentAsync(request, cancellationToken);

            if (result.IsSuccessful)
            {
                _logger.LogInformation(
                    "استرداد وجه با موفقیت انجام شد. پرداخت: {PaymentId}, مبلغ: {Amount}, شناسه استرداد: {RefundId}",
                    paymentId, refundAmount, result.RefundTrackingId);

                // بروزرسانی وضعیت پرداخت
                payment.MarkAsRefunded(result.RefundTrackingId);

                // اگر این پرداخت منجر به شارژ کیف پول شده، باید از موجودی کسر شود
                if (payment.TransactionId.HasValue)
                {
                    // ایجاد تراکنش کسر از موجودی
                    var transaction = await _dbContext.Transactions
                        .FirstOrDefaultAsync(t => t.Id == payment.TransactionId, cancellationToken);

                    if (transaction != null)
                    {
                        await _unitOfWork.BeginTransactionAsync(cancellationToken);

                        try
                        {
                            // پیدا کردن حساب
                            var currencyAccount = await _dbContext.CurrencyAccount
                                .FirstOrDefaultAsync(a => a.Id == transaction.AccountInfoId, cancellationToken);

                            if (currencyAccount != null && currencyAccount.IsActive)
                            {
                                // برداشت معادل مبلغ استرداد از حساب
                                var withdrawTransaction = currencyAccount.Withdraw(
                                    refundAmount,
                                    TransactionType.Refund,
                                    $"برگشت وجه بابت استرداد پرداخت - {reason}",
                                    null);

                                // ثبت ارتباط بین تراکنش‌ها
                                withdrawTransaction.SetRelatedTransactionId(transaction.Id);

                                // بروزرسانی حساب
                                _walletRepository.Update(wallet);
                            }

                            await _unitOfWork.CommitTransactionAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                            _logger.LogError(ex, "خطا در کسر موجودی پس از استرداد. پرداخت: {PaymentId}", paymentId);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "استرداد وجه ناموفق بود. پرداخت: {PaymentId}, خطا: {Error}",
                    paymentId, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در استرداد وجه. پرداخت: {PaymentId}", paymentId);
            return PaymentRefundResult.Failure("خطای داخلی در استرداد وجه", PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentDetailsDto> GetPaymentDetailsAsync(
        Guid paymentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // یافتن رکورد پرداخت
        var payment = await _dbContext.Payments
            .Include(p => p.Transaction)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("درخواست اطلاعات پرداخت نامعتبر. شناسه: {PaymentId}", paymentId);
            throw new KeyNotFoundException($"پرداخت با شناسه {paymentId} یافت نشد");
        }

        // بررسی تطابق کاربر با مالک پرداخت
        var wallet = await _walletRepository.GetByIdAsync(payment.WalletId, cancellationToken);
        if (wallet == null || wallet.UserId != userId)
        {
            _logger.LogWarning("درخواست اطلاعات پرداخت توسط کاربر غیرمجاز. پرداخت: {PaymentId}, کاربر: {UserId}", paymentId, userId);
            throw new UnauthorizedAccessException("شما مجاز به دسترسی به این پرداخت نیستید");
        }

        // تبدیل به DTO
        return new PaymentDetailsDto
        {
            Id = payment.Id,
            UserId = userId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Description = payment.Description,
            GatewayType = payment.GatewayType,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt,
            PaidAt = payment.PaidAt,
            ReferenceId = payment.ReferenceId,
            Authority = payment.Authority,
            OrderId = null, // یا درصورت امکان استخراج از AdditionalData
            AdditionalData = payment.AdditionalData,
            WalletTransactionId = payment.TransactionId
        };
    }

    public async Task<PaymentHistoryResult> GetPaymentHistoryAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // یافتن کیف پول کاربر
        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet == null)
        {
            _logger.LogWarning("درخواست تاریخچه پرداخت برای کاربر بدون کیف پول: {UserId}", userId);
            return new PaymentHistoryResult
            {
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Items = new List<PaymentDetailsDto>()
            };
        }

        // دریافت پرداخت‌های کیف پول با صفحه‌بندی
        var query = _dbContext.Payments
            .Where(p => p.WalletId == wallet.Id)
            .OrderByDescending(p => p.CreatedAt);

        // محاسبه تعداد کل
        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // اعمال صفحه‌بندی
        var payments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // تبدیل به DTO
        var items = payments.Select(p => new PaymentDetailsDto
        {
            Id = p.Id,
            UserId = userId,
            Amount = p.Amount,
            Currency = p.Currency,
            Description = p.Description,
            GatewayType = p.GatewayType,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            PaidAt = p.PaidAt,
            ReferenceId = p.ReferenceId,
            Authority = p.Authority,
            OrderId = null, // یا درصورت امکان استخراج از AdditionalData
            AdditionalData = p.AdditionalData,
            WalletTransactionId = p.TransactionId
        }).ToList();

        return new PaymentHistoryResult
        {
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            Items = items
        };
    }

    public async Task<IntegratedPaymentResult> CreateIntegratedPaymentAsync(
        Guid userId,
        decimal amount,
        CurrencyCode currency,
        string description,
        PaymentGatewayType gatewayType,
        string callbackUrl,
        string orderId,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ایجاد درخواست پرداخت
            var paymentResult = await CreatePaymentRequestAsync(
                userId,
                amount,
                currency,
                description,
                gatewayType,
                callbackUrl,
                metadata,
                orderId,
                null,
                cancellationToken);

            if (!paymentResult.IsSuccessful)
            {
                return new IntegratedPaymentResult
                {
                    IsSuccessful = false,
                    ErrorMessage = paymentResult.ErrorMessage
                };
            }

            // ذخیره اطلاعات لازم برای مرحله بعدی (استفاده در callback)
            var metadataDict = metadata ?? new Dictionary<string, string>();
            metadataDict["IntegratedPayment"] = "true";
            metadataDict["OrderId"] = orderId;

            return new IntegratedPaymentResult
            {
                IsSuccessful = true,
                PaymentUrl = paymentResult.PaymentUrl,
                Authority = paymentResult.Authority
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ایجاد پرداخت یکپارچه. کاربر: {UserId}, مبلغ: {Amount}", userId, amount);
            return new IntegratedPaymentResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطای داخلی در ایجاد پرداخت یکپارچه"
            };
        }
    }
}