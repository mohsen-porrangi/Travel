using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Application.Accounts.Queries.GetUserAccounts;

public class GetUserAccountsQueryHandler(IWalletRepository walletRepository)
    : IQueryHandler<GetUserAccountsQuery, UserAccountsResponse>
{
    public async Task<UserAccountsResponse> Handle(GetUserAccountsQuery request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول یافت نشد", request.UserId);

        // تبدیل حساب‌ها به DTO
        var accountDtos = wallet.Accounts
            .Where(a => !a.IsDeleted)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Currency = a.Currency,
                Balance = a.Balance,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            })
            .ToList();

        return new UserAccountsResponse
        {
            WalletId = wallet.Id,
            TotalAccounts = accountDtos.Count,
            Accounts = accountDtos
        };
    }
}