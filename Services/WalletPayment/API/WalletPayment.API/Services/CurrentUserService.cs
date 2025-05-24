using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;

namespace WalletPayment.API.Services;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{    

    public Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.Items["CurrentUserId"] as Guid?;

        return userId ?? throw new UnauthorizedDomainException("کاربر احراز هویت نشده است");
    }
    public Guid GetCurrentUserAccountId()
    {

        //TODO impeliment
        return new Guid();
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
