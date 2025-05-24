using BuildingBlocks.Contracts;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using WalletPayment.API.Models.Transaction;
using WalletPayment.Application.Transactions.Commands.ProcessTransaction;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Transaction;

public class WalletTransactionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // اندپوینت یکپارچه برای تراکنش‌های کیف پول (واریز/برداشت)
        app.MapPost("api/wallets/transactions", async (
            [FromBody] CreateWalletTransactionRequest request,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var command = new ProcessWalletTransactionCommand(
                userId,
                request.Amount,
                request.Currency,
                request.Direction,
                request.ReferenceId,
                request.OrderId,
                request.Description);

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CreateWalletTransaction")
        .WithDescription("ایجاد تراکنش کیف پول (واریز یا برداشت)")
        .Produces<WalletTransactionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags("Wallet Transactions")
        .RequireAuthorization();    
   
    }
}