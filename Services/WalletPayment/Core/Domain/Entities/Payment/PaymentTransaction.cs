using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using WalletPayment.Domain.Entities.Enums;

namespace Domain.Entities.Payment;

public class PaymentTransaction : EntityWithDomainEvents<Guid>, ISoftDelete
{
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentGatewayType GatewayType { get; private set; }
    public string GatewayToken { get; private set; } = string.Empty;
    public PaymentTransactionStatus Status { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? OrderId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? WalletTransactionId { get; private set; }
    
    private PaymentTransaction() { }
    
    public PaymentTransaction(
        Guid userId,
        decimal amount,
        PaymentGatewayType gatewayType,
        string gatewayToken,
        string description,
        string? orderId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        GatewayType = gatewayType;
        GatewayToken = gatewayToken;
        Status = PaymentTransactionStatus.Pending;
        Description = description;
        OrderId = orderId;        
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // متدی برای به‌روزرسانی وضعیت تراکنش
    public void UpdateStatus(
        PaymentTransactionStatus status, 
        string? referenceId = null, 
        string? gatewayResponse = null)
    {
        Status = status;
        
        if (!string.IsNullOrEmpty(referenceId))
            ReferenceId = referenceId;
            
        if (!string.IsNullOrEmpty(gatewayResponse))
            GatewayResponse = gatewayResponse;
            
        if (status == PaymentTransactionStatus.Successful || 
            status == PaymentTransactionStatus.Failed || 
            status == PaymentTransactionStatus.Canceled)
        {
            CompletedAt = DateTime.UtcNow;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    // متدی برای ثبت تراکنش موفق کیف پول
    public void SetWalletTransaction(Guid walletTransactionId)
    {
        WalletTransactionId = walletTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    // متدهای کمکی برای بررسی وضعیت
    public bool IsPending() => Status == PaymentTransactionStatus.Pending;
    public bool IsCompleted() => Status == PaymentTransactionStatus.Successful 
                             || Status == PaymentTransactionStatus.Failed 
                             || Status == PaymentTransactionStatus.Canceled;
    public bool IsSuccessful() => Status == PaymentTransactionStatus.Successful;
}