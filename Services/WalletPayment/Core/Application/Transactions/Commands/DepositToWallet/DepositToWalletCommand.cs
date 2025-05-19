using BuildingBlocks.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Domain.Entities.Enums;

namespace Application.Transactions.Commands.DepositToWallet;
public record DepositToWalletCommand(
    Guid UserId,
    decimal Amount,
    CurrencyCode Currency,
    string PaymentReferenceId,
    string Description) : ICommand<DepositToWalletResponse>;

public record DepositToWalletResponse(
    Guid TransactionId,
    Guid WalletId,
    Guid AccountId,
    decimal Amount,
    decimal NewBalance,
    DateTime TransactionDate);