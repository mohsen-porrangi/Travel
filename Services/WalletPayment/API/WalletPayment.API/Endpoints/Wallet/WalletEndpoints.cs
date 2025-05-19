using Application.Wallet.Commands.CreateWallet;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WalletPayment.API.Endpoints.Wallet;
public class WalletEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/wallets", async (
            [FromBody] CreateWalletCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Wallets")
        .RequireAuthorization();
    }
}