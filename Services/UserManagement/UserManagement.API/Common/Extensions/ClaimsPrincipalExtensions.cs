using System.Security.Claims;

namespace UserManagement.API.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetIdentityId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new UnauthorizedAccessException("IdentityId not found in token");

            return Guid.Parse(value);
        }
    }
}
