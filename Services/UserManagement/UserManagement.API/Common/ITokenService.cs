using UserManagement.API.Infrastructure.Data.Models;

namespace UserManagement.API.Common
{
    public interface ITokenService
    {
        string GenerateToken(User user, IEnumerable<string> permissionCodes);
        bool ValidateToken(string token);
    }
}
