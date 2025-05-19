using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using WalletPayment.Domain.Entities.Account;
using WalletPayment.Domain.Entities.Enums;

namespace Domain.Entities.Account;
public class AccountBalanceSnapshot : EntityWithDomainEvents, ISoftDelete
{
    public Guid AccountInfoId { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime SnapshotDate { get; private set; }
    public SnapshotType Type { get; private set; }

    // Navigation property
    public AccountInfo Account { get; private set; }

    // کانستراکتور خصوصی برای EF Core
    private AccountBalanceSnapshot() { }

    // کانستراکتور اصلی
    public AccountBalanceSnapshot(Guid accountInfoId, decimal balance, SnapshotType type)
    {
        Id = Guid.NewGuid();
        AccountInfoId = accountInfoId;
        Balance = balance;
        SnapshotDate = DateTime.UtcNow;
        Type = type;
        IsActive = true;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}