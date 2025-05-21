using BuildingBlocks.CQRS;
using System;

namespace WalletPayment.Application.Accounts.Commands.DeleteBankAccount;

public record DeleteBankAccountCommand(Guid UserId, Guid BankAccountId) : ICommand;