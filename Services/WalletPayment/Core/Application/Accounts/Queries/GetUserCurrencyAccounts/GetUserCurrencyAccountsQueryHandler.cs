using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Application.Accounts.Queries.GetUserCurrencyAccounts;

public class GetUserCurrencyAccountsQueryHandler(IWalletRepository walletRepository)
    : IQueryHandler<GetUserCurrencyAccountsQuery, UserCurrencyAccountsResponse>
{
    public async Task<UserCurrencyAccountsResponse> Handle(GetUserCurrencyAccountsQuery request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول یافت نشد", request.UserId);

        // تبدیل حساب‌ها به DTO
        var accountDtos = wallet.CurrencyAccount
            .Where(a => !a.IsDeleted)
            .Select(a => new CurrencyAccountDto
            {
                Id = a.Id,                
                Currency = a.Currency,
                Balance = a.Balance,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            })
            .ToList();

        return new UserCurrencyAccountsResponse
        {
            WalletId = wallet.Id,
            TotalAccounts = accountDtos.Count,
            Accounts = accountDtos
        };
    }
}