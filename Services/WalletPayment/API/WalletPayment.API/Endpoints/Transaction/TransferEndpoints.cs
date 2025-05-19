using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Transactions.Commands.TransferMoney;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Transaction;

public class TransferEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/wallets/transfer", async (
            [FromBody] TransferMoneyCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Transfers")
        .RequireAuthorization();
    }
}