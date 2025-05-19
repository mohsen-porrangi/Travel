using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Accounts.Queries.GetUserAccounts;

namespace WalletPayment.API.Endpoints.Account;

public class AccountEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/wallets/{userId:guid}/accounts", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUserAccountsQuery(userId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Accounts")
        .RequireAuthorization();
    }
}