using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;

namespace WalletPayment.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.Items["CurrentUserId"] as Guid?;

        return userId ?? throw new UnauthorizedDomainException("کاربر احراز هویت نشده است");
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
