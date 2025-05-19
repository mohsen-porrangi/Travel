using Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.Services;

public class PaymentTransactionService : IPaymentTransactionService
{
    private readonly IWalletDbContext _dbContext;
    private readonly ILogger<PaymentTransactionService> _logger;

    public PaymentTransactionService(
        IWalletDbContext dbContext,
        ILogger<PaymentTransactionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> CreatePaymentTransactionAsync(
        Guid userId,
        decimal amount,
        PaymentGatewayType gatewayType,
        string description,
        string? orderId = null,
        CancellationToken cancellationToken = default)
    {
        // ایجاد تراکنش پرداخت جدید
        var transaction = new PaymentTransaction(
            userId,
            amount,
            gatewayType,
            Guid.NewGuid().ToString(), // توکن یکتا
            description,
            orderId
        );

        await _dbContext.PaymentTransactions.AddAsync(transaction, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "تراکنش پرداخت جدید ایجاد شد: {TransactionId}, کاربر: {UserId}, مبلغ: {Amount}",
            transaction.Id, userId, amount);

        return transaction.Id;
    }

    public async Task<bool> UpdatePaymentTransactionStatusAsync(
        Guid transactionId,
        Domain.Entities.Enums.PaymentTransactionStatus status,
        string? referenceId = null,
        string? gatewayResponse = null,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.PaymentTransactions.FindAsync(new object[] { transactionId }, cancellationToken);
        if (transaction == null)
        {
            _logger.LogWarning("تراکنش پرداخت برای به‌روزرسانی یافت نشد: {TransactionId}", transactionId);
            return false;
        }

        // به‌روزرسانی وضعیت تراکنش
        transaction.UpdateStatus(status, referenceId, gatewayResponse);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "وضعیت تراکنش پرداخت به‌روزرسانی شد: {TransactionId}, وضعیت: {Status}",
            transactionId, status);

        return true;
    }

    public async Task<PaymentTransactionDto?> GetPaymentTransactionAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.PaymentTransactions.FindAsync(new object[] { transactionId }, cancellationToken);
        if (transaction == null)
            return null;

        return MapToDto(transaction);
    }

    public async Task<PaymentTransactionDto?> GetPaymentTransactionByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.PaymentTransactions
            .FirstOrDefaultAsync(t => t.GatewayToken == token, cancellationToken);

        if (transaction == null)
            return null;

        return MapToDto(transaction);
    }

    public async Task<bool> CompleteSuccessfulPaymentAsync(
        Guid transactionId,
        string referenceId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.PaymentTransactions.FindAsync(new object[] { transactionId }, cancellationToken);
        if (transaction == null)
        {
            _logger.LogWarning("تراکنش پرداخت برای تکمیل یافت نشد: {TransactionId}", transactionId);
            return false;
        }

        // تغییر وضعیت به موفق
        transaction.UpdateStatus(Domain.Entities.Enums.PaymentTransactionStatus.Successful, referenceId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "تراکنش پرداخت با موفقیت تکمیل شد: {TransactionId}, شناسه مرجع: {ReferenceId}",
            transactionId, referenceId);

        return true;
    }

    private PaymentTransactionDto MapToDto(PaymentTransaction transaction)
    {
        return new PaymentTransactionDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            Amount = transaction.Amount,
            GatewayType = transaction.GatewayType,
            GatewayToken = transaction.GatewayToken,
            Status = transaction.Status,
            ReferenceId = transaction.ReferenceId,
            Description = transaction.Description,
            OrderId = transaction.OrderId,
            CreatedAt = transaction.CreatedAt,
            CompletedAt = transaction.CompletedAt,
            WalletTransactionId = transaction.WalletTransactionId
        };
    }


}