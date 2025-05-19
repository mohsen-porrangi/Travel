using BuildingBlocks.Contracts.Security;
using Microsoft.EntityFrameworkCore;
using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.AccessControl.Services;

public class UserPermissionService : IPermissionService
{
    private readonly AppDbContext _db;

    public UserPermissionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await _db.UserRoles
           .Where(ur => ur.UserId == userId)
           .SelectMany(ur => _db.RolePermissions
               .Where(rp => rp.RoleId == ur.RoleId)
               .Select(rp => rp.Permission.Code))
           .Distinct()
           .ToListAsync();

        return permissions;
    }
}
