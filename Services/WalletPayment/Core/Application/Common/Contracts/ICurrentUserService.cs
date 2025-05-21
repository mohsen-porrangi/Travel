namespace WalletPayment.Application.Common.Contracts;
public interface ICurrentUserService
{
    Guid GetCurrentUserId();
    bool IsAuthenticated { get; }
}
