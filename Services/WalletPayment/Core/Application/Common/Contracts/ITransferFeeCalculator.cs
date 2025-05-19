using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Common.Contracts;

public interface ITransferFeeCalculator
{
    decimal CalculateTransferFee(decimal amount, CurrencyCode currency);
}