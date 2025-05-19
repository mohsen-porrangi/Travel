using Domain.Entities.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Domain.Entities.Enums;

namespace Application.Common.Contracts
{
    public interface IAccountSnapshotService
    {
        /// <summary>
        /// ایجاد اسنپ‌شات از یک حساب مشخص
        /// </summary>
        Task CreateSnapshotAsync(Guid accountId, SnapshotType type, CancellationToken cancellationToken = default);

        /// <summary>
        /// ایجاد اسنپ‌شات از همه حساب‌های فعال
        /// </summary>
        Task CreateSnapshotsForAllAccountsAsync(SnapshotType type, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تاریخچه اسنپ‌شات‌های یک حساب
        /// </summary>
        Task<IEnumerable<AccountBalanceSnapshot>> GetAccountSnapshotsAsync(
            Guid accountId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            SnapshotType? type = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت آخرین اسنپ‌شات حساب
        /// </summary>
        Task<AccountBalanceSnapshot?> GetLastSnapshotAsync(
            Guid accountId,
            SnapshotType? type = null,
            CancellationToken cancellationToken = default);
    }
}
