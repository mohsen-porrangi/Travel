using BuildingBlocks.Contracts;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Accounts.Queries.GetUserCurrencyAccounts;

namespace WalletPayment.API.Endpoints.Account;

public class CurrencyAccountEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/wallets/accounts", async (
            [FromServices] ICurrentUserService currentUserService,            
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUserCurrencyAccountsQuery(currentUserService.GetCurrentUserId()), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetUserAccounts")
.WithDescription("دریافت لیست تمام حساب‌های ارزی کیف پول کاربر")
.Produces<UserCurrencyAccountsResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithTags("Accounts")
        .RequireAuthorization();
    }
}