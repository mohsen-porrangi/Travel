using BuildingBlocks.CQRS;

namespace WalletPayment.Application.Transactions.Commands.RefundTransaction;

public record RefundTransactionCommand : ICommand<RefundTransactionResponse>
{
    public Guid OriginalTransactionId { get; init; }
    public decimal? Amount { get; init; } // null برای استرداد کامل
    public string Reason { get; init; } = "استرداد وجه";
    public bool IsAdminApproved { get; init; } = false; // آیا توسط ادمین تأیید شده است
}

public record RefundTransactionResponse
{
    public Guid RefundTransactionId { get; init; }
    public Guid OriginalTransactionId { get; init; }
    public decimal RefundedAmount { get; init; }
    public decimal NewAccountBalance { get; init; }
    public DateTime RefundDate { get; init; }
    public bool IsPartial { get; init; }
}