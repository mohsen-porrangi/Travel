using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Application.Common.Contracts;

namespace Application.Wallet.Queries.GetWalletByUserId;
public class GetWalletByUserIdQueryHandler(IWalletRepository walletRepository)
    : IQueryHandler<GetWalletByUserIdQuery, WalletDetailsDto>
{
    public async Task<WalletDetailsDto> Handle(GetWalletByUserIdQuery request, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        return new WalletDetailsDto(
            wallet.Id,
            wallet.UserId,
            wallet.CreditLimit,
            wallet.CreditBalance,
            wallet.CreditDueDate,
            wallet.IsActive,
            wallet.Accounts.Select(a => new AccountDto(
                a.Id,
                a.AccountNumber,
                a.Currency,
                a.Balance
            )).ToList()
        );
    }
}