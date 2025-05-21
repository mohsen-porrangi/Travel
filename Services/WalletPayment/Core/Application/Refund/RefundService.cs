using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Common.Models;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Application.Transactions.Commands.RefundTransaction;
using WalletPayment.Application.Transactions.Queries.GetRefundableTransaction;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.Services;

public class RefundService(
    IPaymentService paymentService,
    ISender mediator,
    ILogger<RefundService> logger) : IRefundService
{
    public async Task<RefundabilityResult> CheckRefundabilityAsync(
        Guid userId,
        Guid? transactionId,
        Guid? paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (transactionId.HasValue)
            {
                // بررسی قابلیت استرداد تراکنش
                var query = new GetRefundableTransactionQuery(transactionId.Value);
                var result = await mediator.Send(query, cancellationToken);

                return new RefundabilityResult
                {
                    IsRefundable = true,
                    SourceType = RefundSourceType.Transaction,
                    SourceId = result.TransactionId,
                    OriginalAmount = result.OriginalAmount,
                    RefundableAmount = result.RefundableAmount,
                    AlreadyRefundedAmount = result.AlreadyRefundedAmount,
                    HasPartialRefunds = result.HasPartialRefunds,
                    RefundHistory = result.RefundHistory,
                    Currency = result.Currency.ToString(),
                    TransactionDate = result.TransactionDate,
                    Description = result.Description
                };
            }
            else if (paymentId.HasValue)
            {
                // بررسی قابلیت استرداد پرداخت
                var paymentDetails = await paymentService.GetPaymentDetailsAsync(
                    paymentId.Value, userId, cancellationToken);

                // بررسی وضعیت پرداخت
                bool isRefundable = paymentDetails.Status == "Verified" ||
                                    paymentDetails.Status == "Paid";

                return new RefundabilityResult
                {
                    IsRefundable = isRefundable,
                    SourceType = RefundSourceType.Payment,
                    SourceId = paymentDetails.Id,
                    OriginalAmount = paymentDetails.Amount,
                    RefundableAmount = paymentDetails.Amount, // برای پرداخت‌ها فعلاً فقط استرداد کامل پشتیبانی می‌شود
                    AlreadyRefundedAmount = 0, // اطلاعات استردادهای قبلی را نداریم
                    HasPartialRefunds = false,
                    RefundHistory = new List<RefundHistoryItemDto>(), // اطلاعات استردادهای قبلی را نداریم
                    Currency = paymentDetails.Currency.ToString(),
                    TransactionDate = paymentDetails.PaidAt,
                    Description = paymentDetails.Description
                };
            }

            throw new BadRequestException("باید حداقل یکی از شناسه تراکنش یا شناسه پرداخت ارائه شود");
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "تراکنش یا پرداخت مورد نظر برای بررسی استرداد یافت نشد");

            return new RefundabilityResult
            {
                IsRefundable = false,
                SourceType = transactionId.HasValue ? RefundSourceType.Transaction : RefundSourceType.Payment,
                SourceId = transactionId ?? paymentId ?? Guid.Empty,
                OriginalAmount = 0,
                RefundableAmount = 0,
                AlreadyRefundedAmount = 0,
                HasPartialRefunds = false,
                RefundHistory = new List<RefundHistoryItemDto>()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در بررسی قابلیت استرداد تراکنش یا پرداخت");
            throw;
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(
        Guid userId,
        Guid? transactionId,
        Guid? paymentId,
        decimal? amount,
        string reason,
        bool isAdminApproved = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (transactionId.HasValue)
            {
                // استرداد تراکنش
                var command = new RefundTransactionCommand
                {
                    OriginalTransactionId = transactionId.Value,
                    Amount = amount,
                    Reason = reason,
                    IsAdminApproved = isAdminApproved
                };

                var result = await mediator.Send(command, cancellationToken);

                return new RefundResult
                {
                    IsSuccessful = true,
                    RefundId = result.RefundTransactionId,
                    SourceType = RefundSourceType.Transaction,
                    SourceId = result.OriginalTransactionId,
                    RefundedAmount = result.RefundedAmount,
                    RemainingBalance = result.NewAccountBalance,
                    RefundDate = result.RefundDate,
                    IsPartial = result.IsPartial
                };
            }
            else if (paymentId.HasValue)
            {
                // استرداد پرداخت
                var result = await paymentService.RefundPaymentAsync(
                    paymentId.Value, userId, reason, amount, cancellationToken);

                if (result.IsSuccessful)
                {
                    return new RefundResult
                    {
                        IsSuccessful = true,
                        RefundId = Guid.Parse(result.RefundTrackingId!),
                        SourceType = RefundSourceType.Payment,
                        SourceId = paymentId.Value,
                        RefundedAmount = amount ?? 0, // باید از نتیجه استرداد دریافت شود
                        RemainingBalance = 0, // اطلاعات موجودی جدید در دسترس نیست
                        RefundDate = DateTime.UtcNow,
                        IsPartial = amount.HasValue // اگر مبلغ مشخص شده باشد، استرداد جزئی است
                    };
                }
                else
                {
                    return new RefundResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = result.ErrorMessage,
                        SourceType = RefundSourceType.Payment,
                        SourceId = paymentId.Value,
                        RefundDate = DateTime.UtcNow
                    };
                }
            }

            throw new BadRequestException("باید حداقل یکی از شناسه تراکنش یا شناسه پرداخت ارائه شود");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در انجام استرداد تراکنش یا پرداخت");

            return new RefundResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                SourceType = transactionId.HasValue ? RefundSourceType.Transaction : RefundSourceType.Payment,
                SourceId = transactionId ?? paymentId ?? Guid.Empty,
                RefundDate = DateTime.UtcNow
            };
        }
    }
}