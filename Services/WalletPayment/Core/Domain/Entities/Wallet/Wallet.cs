using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using Domain.Domain.Events;
using WalletPayment.Domain.Entities.Account;
using WalletPayment.Domain.Entities.Credit;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Domain.Events;

namespace WalletPayment.Domain.Entities.Wallet;
public class Wallet : EntityWithDomainEvents, IAggregateRoot, ISoftDelete
{
    public Guid UserId { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal CreditBalance { get; private set; }
    public DateTime? CreditDueDate { get; private set; }

    private readonly List<AccountInfo> _accounts = new();
    private readonly List<CreditHistory> _creditHistory = new();
    public IReadOnlyCollection<CreditHistory> CreditHistory => _creditHistory.AsReadOnly();
    public IReadOnlyCollection<AccountInfo> Accounts => _accounts.AsReadOnly();

    // کانستراکتور خصوصی برای EF Core
    private Wallet() { }

    // کانستراکتور برای ایجاد یک کیف پول جدید
    public Wallet(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreditLimit = 0;
        CreditBalance = 0;
        CreditDueDate = null;
        IsActive = true;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WalletCreatedEvent(Id, userId));
    }
    // ایجاد یک حساب جدید برای ارز مشخص
    public AccountInfo CreateAccount(CurrencyCode currency, string accountNumber)
    {
        if (_accounts.Any(a => a.Currency == currency && !a.IsDeleted))
            throw new InvalidOperationException($"حساب با ارز {currency} قبلاً برای این کیف پول ایجاد شده است");

        var account = new AccountInfo(this.Id, currency, accountNumber);
        _accounts.Add(account);

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AccountCreatedEvent(Id, account.Id, currency));

        return account;
    }
    // متدهای قبلی حفظ شوند، متدهای زیر اضافه شوند

    // اختصاص اعتبار به کیف پول
    public void AssignCredit(decimal amount, DateTime dueDate, string description)
    {
        if (amount <= 0)
            throw new ArgumentException("مبلغ اعتبار باید بزرگتر از صفر باشد", nameof(amount));

        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("تاریخ سررسید باید در آینده باشد", nameof(dueDate));

        CreditLimit = amount;
        CreditBalance = amount;
        CreditDueDate = dueDate;

        var creditHistory = new CreditHistory(
            Id,
            amount,
            DateTime.UtcNow,
            dueDate,
            null,
            CreditStatus.Active,
            description);

        _creditHistory.Add(creditHistory);

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CreditAssignedEvent(Id, UserId, amount, dueDate));
    }

    // استفاده از اعتبار
    public bool UseCredit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("مبلغ باید بزرگتر از صفر باشد", nameof(amount));

        if (CreditBalance < amount || !CreditDueDate.HasValue || CreditDueDate.Value < DateTime.UtcNow)
            return false;

        CreditBalance -= amount;
        UpdatedAt = DateTime.UtcNow;

        return true;
    }

    // تسویه اعتبار
    public void SettleCredit(Guid transactionId)
    {
        var activeCredit = _creditHistory
            .FirstOrDefault(c => c.Status == CreditStatus.Active || c.Status == CreditStatus.Overdue);

        if (activeCredit == null)
            throw new InvalidOperationException("هیچ اعتبار فعالی برای تسویه وجود ندارد");

        activeCredit.Settle(transactionId);

        CreditLimit = 0;
        CreditBalance = 0;
        CreditDueDate = null;

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CreditSettledEvent(Id, activeCredit.Id, activeCredit.Amount));
    }

    // بررسی سررسید اعتبار
    public void CheckCreditDueDate()
    {
        if (!CreditDueDate.HasValue || CreditBalance <= 0)
            return;

        if (CreditDueDate.Value < DateTime.UtcNow)
        {
            var activeCredit = _creditHistory
                .FirstOrDefault(c => c.Status == CreditStatus.Active);

            if (activeCredit != null)
            {
                activeCredit.MarkAsOverdue();
                AddDomainEvent(new CreditOverdueEvent(Id, UserId, CreditBalance, CreditDueDate.Value));
            }
        }
    }
}

