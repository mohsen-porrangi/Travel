using BuildingBlocks.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Domain.Entities.Enums;

namespace Application.Transactions.Commands.WithdrawFromWallet;

public record WithdrawFromWalletCommand(
    Guid UserId,
    decimal Amount,
    CurrencyCode Currency,
    string OrderId,
    string Description) : ICommand<WithdrawFromWalletResponse>;

public record WithdrawFromWalletResponse(
    Guid TransactionId,
    Guid WalletId,
    Guid AccountId,
    decimal Amount,
    decimal RemainingBalance,
    DateTime TransactionDate);