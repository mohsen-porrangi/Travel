using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WalletPayment.Application.Accounts.Commands.CreateBankAccount;
using WalletPayment.Application.Accounts.Commands.DeleteBankAccount;
using WalletPayment.Application.Accounts.Queries.GetUserBankAccounts;

namespace WalletPayment.API.Endpoints.Account;

public class AccountBankEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // 1. دریافت لیست حساب‌های بانکی کاربر
        app.MapGet("/wallets/{userId:guid}/bank-accounts", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUserBankAccountsQuery(userId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("BankAccounts")
        .RequireAuthorization();

        // 2. افزودن حساب بانکی جدید
        app.MapPost("/wallets/{userId:guid}/bank-accounts", async (
            Guid userId,
            [FromBody] CreateBankAccountCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (command.UserId != userId)
            {
                return Results.BadRequest("شناسه کاربر در مسیر و بدنه درخواست باید یکسان باشد");
            }

            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/wallets/{userId}/bank-accounts/{result.BankAccountId}", result);
        })
        .WithTags("BankAccounts")
        .RequireAuthorization();

        // 3. حذف حساب بانکی
        app.MapDelete("/wallets/{userId:guid}/bank-accounts/{accountId:guid}", async (
            Guid userId,
            Guid accountId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteBankAccountCommand(userId, accountId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .WithTags("BankAccounts")
        .RequireAuthorization();
    }
}