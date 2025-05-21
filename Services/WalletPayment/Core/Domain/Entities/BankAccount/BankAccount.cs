using BuildingBlocks.Domain;
using System;

namespace WalletPayment.Domain.Entities.BankAccount;

public class BankAccount : EntityWithDomainEvents
{
    public Guid UserId { get; private set; }
    public string BankName { get; private set; }
    public string AccountNumber { get; private set; }
    public string CardNumber { get; private set; }
    public string ShabaNumber { get; private set; }
    public string AccountHolderName { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }

    
    private BankAccount() { }

    // کانستراکتور عمومی
    public BankAccount(
        Guid userId,
        string bankName,
        string accountNumber,
        string cardNumber,
        string shabaNumber,
        string accountHolderName)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        BankName = bankName;
        AccountNumber = accountNumber;
        CardNumber = cardNumber;
        ShabaNumber = shabaNumber;
        AccountHolderName = accountHolderName;
        IsVerified = false; // در ابتدا حساب تأیید نشده است
        IsDefault = false;
        IsActive = true;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // تنظیم به عنوان حساب پیش‌فرض
    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // برداشتن حالت پیش‌فرض
    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // تنظیم وضعیت تأیید
    public void SetVerificationStatus(bool isVerified)
    {
        IsVerified = isVerified;
        UpdatedAt = DateTime.UtcNow;
    }
}