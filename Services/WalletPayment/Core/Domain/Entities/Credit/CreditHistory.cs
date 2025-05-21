using BuildingBlocks.Domain;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Domain.Entities.Credit;

public class CreditHistory : EntityWithDomainEvents<long>
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime GrantDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? SettlementDate { get; private set; }
    public Guid? SettlementTransactionId { get; private set; }
    public CreditStatus Status { get; private set; }
    public string Description { get; private set; }

    private CreditHistory() { }

    public CreditHistory(
        Guid walletId,
        decimal amount,
        DateTime grantDate,
        DateTime dueDate,
        Guid? settlementTransactionId,
        CreditStatus status,
        string description)
    {        
        WalletId = walletId;
        Amount = amount;
        GrantDate = grantDate;
        DueDate = dueDate;
        SettlementTransactionId = settlementTransactionId;
        Status = status;
        Description = description;        
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // تسویه اعتبار
    public void Settle(Guid transactionId)
    {
        if (Status == CreditStatus.Settled)
            throw new InvalidOperationException("این اعتبار قبلاً تسویه شده است");

        Status = CreditStatus.Settled;
        SettlementDate = DateTime.UtcNow;
        SettlementTransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    // تغییر وضعیت به سررسید گذشته
    public void MarkAsOverdue()
    {
        if (Status != CreditStatus.Active)
            throw new InvalidOperationException("فقط اعتبارهای فعال می‌توانند به وضعیت سررسید گذشته تغییر کنند");

        Status = CreditStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }
}