using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Application.Common.Models;

namespace WalletPayment.Application.Common.Contracts
{
    public interface IRefundService
    {
        Task<RefundabilityResult> CheckRefundabilityAsync(
            Guid userId,
            Guid? transactionId,
            Guid? paymentId,
            CancellationToken cancellationToken);

        Task<RefundResult> ProcessRefundAsync(
            Guid userId,
            Guid? transactionId,
            Guid? paymentId,
            decimal? amount,
            string reason,
            bool isAdminApproved,
            CancellationToken cancellationToken);
    }
}
