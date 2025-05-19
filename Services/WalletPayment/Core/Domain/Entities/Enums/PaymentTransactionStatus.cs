using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Domain.Entities.Enums;
/// <summary>
/// وضعیت تراکنش پرداخت
/// </summary>
public enum PaymentTransactionStatus
{
    Pending = 1,
    Processing = 2,
    Successful = 3,
    Failed = 4,
    Canceled = 5,
    Refunded = 6
}