using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WalletPayment.API.Models.Transaction;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Transactions.Commands.ProcessTransaction;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Transaction;

public class WalletTransactionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // اندپوینت یکپارچه برای تراکنش‌های کیف پول (واریز/برداشت)
        app.MapPost("/wallets/transactions", async (
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
        .WithTags("Wallet Transactions")
        .WithName("CreateWalletTransaction")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "ایجاد تراکنش کیف پول",
            description: "ایجاد تراکنش واریز یا برداشت از کیف پول کاربر"
        ))
        .RequireAuthorization();

        // برای حفظ سازگاری با گذشته، endpoint های قدیمی را نگه می‌داریم
        // اما آنها را با هدایت به endpoint جدید پیاده‌سازی می‌کنیم

        app.MapPost("/wallets/deposit", async (
            [FromBody] LegacyDepositRequest request,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var command = new ProcessWalletTransactionCommand(
                userId,
                request.Amount,
                request.Currency,
                TransactionDirection.In,
                request.PaymentReferenceId,
                null,
                request.Description);

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Wallet Transactions")
        .WithName("DepositToWallet")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "واریز به کیف پول (قدیمی)",
            description: "این endpoint برای حفظ سازگاری با نسخه‌های قبلی نگه داشته شده است. لطفاً از endpoint جدید /wallets/transactions استفاده کنید."
        ))
        .RequireAuthorization();

        app.MapPost("/wallets/withdraw", async (
            [FromBody] LegacyWithdrawRequest request,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var command = new ProcessWalletTransactionCommand(
                userId,
                request.Amount,
                request.Currency,
                TransactionDirection.Out,
                null,
                request.OrderId,
                request.Description);

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Wallet Transactions")
        .WithName("WithdrawFromWallet")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "برداشت از کیف پول (قدیمی)",
            description: "این endpoint برای حفظ سازگاری با نسخه‌های قبلی نگه داشته شده است. لطفاً از endpoint جدید /wallets/transactions استفاده کنید."
        ))
        .RequireAuthorization();
    }
}