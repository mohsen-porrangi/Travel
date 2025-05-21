using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
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

        // اندپوینت‌های زیر برای حفظ سازگاری با قبل نگه داشته شده‌اند و بهتر است در نسخه‌های آینده حذف شوند

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

            // اضافه کردن هشدار در مورد استفاده از endpoint legacy
            var responseData = new
            {
                result,
                message = "این endpoint برای حفظ سازگاری با نسخه‌های قبلی نگه داشته شده است. لطفاً در آینده از endpoint جدید /wallets/transactions استفاده کنید."
            };

            return Results.Ok(responseData);
        })
        .WithTags("Wallet Transactions")
        .WithName("DepositToWallet")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "واریز به کیف پول (Deprecated)",
            description: "این endpoint منسوخ شده است و برای حفظ سازگاری با نسخه‌های قبلی نگه داشته شده است. لطفاً از endpoint جدید /wallets/transactions استفاده کنید."
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

            // اضافه کردن هشدار در مورد استفاده از endpoint legacy
            var responseData = new
            {
                result,
                message = "این endpoint برای حفظ سازگاری با نسخه‌های قبلی نگه داشته شده است. لطفاً در آینده از endpoint جدید /wallets/transactions استفاده کنید."
            };

            return Results.Ok(responseData);
        })
        .WithTags("Wallet Transactions")
        .WithName("WithdrawFromWallet")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "برداشت از کیف پول (Deprecated)",
            description: "این endpoint منسوخ شده است و برای حفظ سازگاری با نسخه‌های قبلی نگه داشته شده است. لطفاً از endpoint جدید /wallets/transactions استفاده کنید."
        ))
        .RequireAuthorization();
    }
}