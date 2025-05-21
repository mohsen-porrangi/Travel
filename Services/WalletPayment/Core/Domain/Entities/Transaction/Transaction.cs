using BuildingBlocks.Domain;
using WalletPayment.Domain.Entities.Enums;
using TransactionStatus = WalletPayment.Domain.Entities.Enums.TransactionStatus;
using TransactionType = WalletPayment.Domain.Entities.Enums.TransactionType;

namespace WalletPayment.Domain.Entities.Transaction;
public class Transaction : EntityWithDomainEvents
{
    public Guid WalletId { get; private set; }
    public Guid AccountInfoId { get; private set; }
    public Guid? RelatedTransactionId { get; private set; }
    
    public void SetRelatedTransactionId(Guid? relatedId)
    {
        RelatedTransactionId = relatedId;
        UpdatedAt = DateTime.UtcNow;
    }
    public decimal Amount { get; private set; }
    public TransactionDirection Direction { get; private set; }
    public TransactionType Type { get; private set; }
    public void SetType(WalletPayment.Domain.Entities.Enums.TransactionType type)
    {
        Type = type;
        UpdatedAt = DateTime.UtcNow;
    }
    public TransactionStatus Status { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public CurrencyCode Currency { get; private set; }
    public string Description { get; private set; }
    public bool IsCredit { get; private set; }
    public DateTime? DueDate { get; private set; }
    public string PaymentReferenceId { get; private set; }
    public string OrderId { get; private set; }

    
    private Transaction() { }
    public void SetStatus(WalletPayment.Domain.Entities.Enums.TransactionStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
  
    public Transaction(
        Guid accountInfoId,
        Guid walletId,
        decimal amount,
        TransactionDirection direction,
        WalletPayment.Domain.Entities.Enums.TransactionType type,
        WalletPayment.Domain.Entities.Enums.TransactionStatus status,
        CurrencyCode currency,
        string description,
        bool isCredit = false,
        DateTime? dueDate = null,
        string paymentReferenceId = null,
        string orderId = null,
        Guid? relatedTransactionId = null)
    {
        Id = Guid.NewGuid();
        AccountInfoId = accountInfoId;
        WalletId = walletId;
        Amount = amount;
        Direction = direction;
        Type = type;
        Status = status;
        TransactionDate = DateTime.UtcNow;
        Currency = currency;
        Description = description;
        IsCredit = isCredit;
        DueDate = dueDate;
        PaymentReferenceId = paymentReferenceId;
        OrderId = orderId;
        RelatedTransactionId = relatedTransactionId;
        IsActive = true;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // تغییر وضعیت تراکنش
    public void ChangeStatus(TransactionStatus newStatus)
    {
        if (Status == TransactionStatus.Completed && newStatus != TransactionStatus.Refunded)
            throw new InvalidOperationException("تراکنش تکمیل شده قابل تغییر نیست");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void LinkToTransfer(Guid transferId)
    {       
        RelatedTransactionId = transferId;
        UpdatedAt = DateTime.UtcNow;
    }
    // <summary>
    /// بررسی امکان استرداد تراکنش
    /// </summary>
    public bool IsRefundable()
    {
        // تراکنش‌های خروجی (خرید) و کامل شده قابل استرداد هستند
        return Direction == TransactionDirection.Out &&
               Status == TransactionStatus.Completed;
    }

    /// <summary>
    /// بررسی اینکه آیا تراکنش یک استرداد است
    /// </summary>
    public bool IsRefund()
    {
        return Type == TransactionType.Refund &&
               RelatedTransactionId.HasValue;
    }
}