using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Accounts.Queries.GetUserAccounts;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.API.Endpoints.Account;

public class AccountEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/wallets/accounts", async (
            [FromServices] ICurrentUserService currentUserService,
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUserAccountsQuery(currentUserService.GetCurrentUserId()), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Accounts")
        .RequireAuthorization();
    }
}