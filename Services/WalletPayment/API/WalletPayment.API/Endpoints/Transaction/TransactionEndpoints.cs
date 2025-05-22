
using BuildingBlocks.Contracts;
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

        app.MapGet("/wallets/transactions", async (
            [FromServices] ICurrentUserService currentUserService,
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
                UserId = currentUserService.GetCurrentUserId(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                SortBy = sortBy,
                SortDesc = sortDesc
            };

            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
      .WithName("GetUserTransactions")
.WithDescription("دریافت لیست تراکنش‌های کیف پول با فیلتر، مرتب‌سازی و صفحه‌بندی")
.Produces<PaginatedList<TransactionDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.WithTags("Transactions")
      .RequireAuthorization();

        app.MapGet("/wallets/statement", async (
            [FromServices] ICurrentUserService currentUserService,
            ISender sender,
            CancellationToken cancellationToken,
            [FromQuery] CurrencyCode currency = CurrencyCode.IRR,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
            ) =>
        {
            var query = new GetAccountStatementQuery
            {
                UserId = currentUserService.GetCurrentUserId(),
                Currency = currency,
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAccountStatement")
.WithDescription("دریافت صورتحساب مفصل حساب ارزی در بازه زمانی مشخص")
.Produces<AccountStatementResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithTags("Transactions")
        .RequireAuthorization();

        app.MapGet("/wallets/summary", async (
            [FromServices] ICurrentUserService currentUserService,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var result = await sender.Send(new GetWalletSummaryQuery(currentUserService.GetCurrentUserId()), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetWalletSummary")
.WithDescription("دریافت خلاصه کامل کیف پول شامل موجودی، اعتبار و آمار تراکنش‌ها")
.Produces<WalletSummaryResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithTags("Wallets")
        .RequireAuthorization();
    }
}
