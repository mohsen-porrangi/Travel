using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Application.Credit.Queries.GetCreditStatus;

public class GetCreditStatusQueryHandler(IWalletRepository walletRepository)
    : IQueryHandler<GetCreditStatusQuery, CreditStatusResponse>
{
    public async Task<CreditStatusResponse> Handle(GetCreditStatusQuery request, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdWithCreditHistoryAsync(request.UserId, cancellationToken);

        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        // بررسی وضعیت سررسید
        wallet.CheckCreditDueDate();

        bool isOverdue = wallet.CreditDueDate.HasValue && wallet.CreditDueDate.Value < DateTime.UtcNow;

        return new CreditStatusResponse(
            wallet.Id,
            wallet.CreditLimit,
            wallet.CreditBalance,
            wallet.CreditLimit - wallet.CreditBalance, // مقدار استفاده شده
            wallet.CreditDueDate,
            isOverdue,
            wallet.CreditHistory.Select(ch => new CreditHistoryDto(
                ch.Id,
                ch.Amount,
                ch.GrantDate,
                ch.DueDate,
                ch.SettlementDate,
                ch.Status,
                ch.Description
            )).ToList()
        );
    }
}