using BuildingBlocks.Domain;
using Domain.Domain.Events;
using WalletPayment.Domain.Entities.Enums;
using static Domain.Domain.Events.WalletDepositedEvent;

namespace WalletPayment.Domain.Entities.Account;
public class AccountInfo : EntityWithDomainEvents
{
    public Guid WalletId { get; private set; }
    public string AccountNumber { get; private set; }
    public CurrencyCode Currency { get; private set; }
    public decimal Balance { get; private set; }

    private readonly List<Transaction.Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction.Transaction> Transactions => _transactions.AsReadOnly();

    // کانستراکتور خصوصی برای EF Core
    private AccountInfo() { }

    // کانستراکتور برای ایجاد یک حساب جدید
    public AccountInfo(Guid walletId, CurrencyCode currency, string accountNumber)
    {
        Id = Guid.NewGuid();
        WalletId = walletId;
        AccountNumber = accountNumber;
        Currency = currency;
        Balance = 0;
        IsActive = true;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // شارژ حساب (افزایش موجودی)
    public Transaction.Transaction Deposit(decimal amount, string description, string referenceId)
    {
        if (amount <= 0)
            throw new ArgumentException("مبلغ شارژ باید بزرگتر از صفر باشد", nameof(amount));

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction.Transaction(
            this.Id,
            this.WalletId,
            amount,
            TransactionDirection.In,
            WalletPayment.Domain.Entities.Enums.TransactionType.Deposit,
            WalletPayment.Domain.Entities.Enums.TransactionStatus.Completed,
            this.Currency,
            description,
            false,
            null,
            referenceId,
            null
        );

        _transactions.Add(transaction);

        AddDomainEvent(new WalletDepositedEvent(WalletId, Id, amount, Currency, referenceId));

        return transaction;
    }
    // برداشت از حساب
    public Transaction.Transaction Withdraw(decimal amount, TransactionType type, string description, string orderId = null)
    {
        if (amount <= 0)
            throw new ArgumentException("مبلغ برداشت باید بزرگتر از صفر باشد", nameof(amount));

        if (Balance < amount)
            throw new InvalidOperationException($"موجودی کافی نیست. موجودی فعلی: {Balance}, مبلغ درخواستی: {amount}");

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction.Transaction(
            this.Id,
            this.WalletId,
            amount,
            TransactionDirection.Out,
            type,
            WalletPayment.Domain.Entities.Enums.TransactionStatus.Completed,
            this.Currency,
            description,
            false,
            null,
            null,
            orderId
        );

        _transactions.Add(transaction);

        AddDomainEvent(new WalletWithdrawnEvent(WalletId, Id, amount, Currency, orderId));

        return transaction;
    }
}