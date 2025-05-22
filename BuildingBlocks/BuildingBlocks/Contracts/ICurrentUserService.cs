namespace BuildingBlocks.Contracts;
public interface ICurrentUserService
{
    Guid GetCurrentUserId();
    bool IsAuthenticated { get; }
}
