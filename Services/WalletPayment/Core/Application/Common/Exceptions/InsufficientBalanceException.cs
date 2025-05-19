using BuildingBlocks.Exceptions;

namespace WalletPayment.Application.Common.Exceptions;

public class InsufficientBalanceException : BadRequestException
{
    public InsufficientBalanceException(Guid walletId, decimal requested, decimal available)
        : base(
            $"موجودی کیف پول کافی نیست",
            $"درخواست برداشت {requested} از کیف پول با شناسه {walletId} با موجودی {available}")
    {
        RequestedAmount = requested;
        AvailableBalance = available;
    }

    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }
}