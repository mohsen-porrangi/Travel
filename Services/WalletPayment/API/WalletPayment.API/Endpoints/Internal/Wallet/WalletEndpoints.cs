using Application.Wallet.Commands.CreateWallet;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WalletPayment.API.Endpoints.Internal.Wallet;
public class WalletEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/internal/wallet", async (
            [FromBody] CreateWalletCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CreateWallet")
        .WithDescription("ایجاد کیف پول جدید برای کاربر")
        .Produces<CreateWalletResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags("Wallets");
       // .RequireAuthorization();
    }
}