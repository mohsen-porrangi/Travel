namespace BuildingBlocks.Contracts;
public interface ICurrentUserService
{
    Guid GetCurrentUserId();
    Guid GetCurrentUserAccountId();
    bool IsAuthenticated { get; }
}
