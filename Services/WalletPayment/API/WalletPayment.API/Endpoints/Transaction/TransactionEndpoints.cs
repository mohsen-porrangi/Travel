using Application.Transactions.Commands.DepositToWallet;
using Application.Transactions.Commands.WithdrawFromWallet;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Transactions.Queries.Common;
using WalletPayment.Application.Transactions.Queries.GetAccountStatement;
using WalletPayment.Application.Transactions.Queries.GetUserTransactionHistory;
using WalletPayment.Application.Wallet.Queries.GetWalletSummary;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Transaction;
public class TransactionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/wallets/deposit", async (
            [FromBody] DepositToWalletCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Transactions")
        .RequireAuthorization();

        app.MapPost("/wallets/withdraw", async (
            [FromBody] WithdrawFromWalletCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Transactions")
        .RequireAuthorization();

        app.MapGet("/wallets/{userId:guid}/transactions", async (
          Guid userId,
          ISender sender,
          CancellationToken cancellationToken,
          [FromQuery] int pageNumber = 1,
          [FromQuery] int pageSize = 20,
          [FromQuery] DateTime? startDate = null,
          [FromQuery] DateTime? endDate = null,
          [FromQuery] TransactionDirection? direction = null,
          [FromQuery] Domain.Entities.Enums.TransactionType? type = null,
          [FromQuery] TransactionStatus? status = null,
          [FromQuery] CurrencyCode? currency = null,
          [FromQuery] decimal? minAmount = null,
          [FromQuery] decimal? maxAmount = null,
          [FromQuery] bool? isCredit = null,
          [FromQuery] string sortBy = "TransactionDate",
          [FromQuery] bool sortDesc = true
          ) =>
        {
            var filter = new TransactionFilter
            {
                StartDate = startDate,
                EndDate = endDate,
                Direction = direction,
                Type = type,
                Status = status,
                Currency = currency,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                IsCredit = isCredit
            };

            var query = new GetUserTransactionHistoryQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                SortBy = sortBy,
                SortDesc = sortDesc
            };

            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
      .WithTags("Transactions")
      .RequireAuthorization();

        app.MapGet("/wallets/{userId:guid}/statement", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken,
            [FromQuery] CurrencyCode currency = CurrencyCode.IRR,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
            ) =>
        {
            var query = new GetAccountStatementQuery
            {
                UserId = userId,
                Currency = currency,
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Transactions")
        .RequireAuthorization();

        app.MapGet("/wallets/{userId:guid}/summary", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var result = await sender.Send(new GetWalletSummaryQuery(userId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Wallets")
        .RequireAuthorization();
    }
}
