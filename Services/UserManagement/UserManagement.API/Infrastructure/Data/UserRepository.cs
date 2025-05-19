// File: Services/UserManagement/UserManagement.API/Infrastructure/Data/UserRepository.cs

using UserManagement.API.Infrastructure.Data;
namespace UserManagement.API.Infrastructure.Data;

public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        return _db.MasterIdentities.AnyAsync(x => x.Email == email);
    }

    public Task<bool> ExistsByMobileAsync(string mobile)
    {
        return _db.MasterIdentities.AnyAsync(x => x.Mobile == mobile);
    }

    public async Task CreateAsync(User user, MasterIdentity identity)
    {
        await _db.MasterIdentities.AddAsync(identity);
        await _db.Users.AddAsync(user);
    }

    public Task UpdateIdentityAsync(MasterIdentity identity)
    {
        _db.MasterIdentities.Update(identity);
        return Task.CompletedTask;
    }

    public Task<MasterIdentity?> GetIdentityByEmailAsync(string email)
    {
        return _db.MasterIdentities.FirstOrDefaultAsync(x => x.Email == email);
    }

    public Task<MasterIdentity?> GetIdentityByMobileAsync(string mobile)
    {
        return _db.MasterIdentities.FirstOrDefaultAsync(x => x.Mobile == mobile);
    }

    public Task<MasterIdentity?> GetIdentityByUsernameAsync(string username)
    {
        return _db.MasterIdentities.FirstOrDefaultAsync(x => x.Email == username || x.Mobile == username);
    }

    public Task<User?> GetUserByIdentityIdAsync(Guid identityId)
    {
        return _db.Users
            .Include(x => x.MasterIdentity)
            .FirstOrDefaultAsync(x => x.IdentityId == identityId);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await (from user in _db.Users
                                 join ur in _db.UserRoles on user.Id equals ur.UserId
                                 join rp in _db.RolePermissions on ur.RoleId equals rp.RoleId
                                 join p in _db.Permissions on rp.PermissionId equals p.Id
                                 where user.Id == userId
                                 select p.Code).ToListAsync();

        return permissions;
    }

    public Task AddLoginHistoryAsync(LoginHistory history)
    {
        _db.LoginHistories.Add(history);
        return Task.CompletedTask;
    }

    public async Task StorePasswordResetTokenAsync(Guid identityId, Guid resetToken)
    {
        var identity = await _db.MasterIdentities.FirstOrDefaultAsync(x => x.Id == identityId);
        if (identity == null)
            throw new InvalidOperationException($"Identity not found. ID: {identityId}");

        identity.ResetToken = resetToken;
        identity.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
    }

    public Task<MasterIdentity?> GetIdentityByResetTokenAsync(Guid resetToken)
    {
        return _db.MasterIdentities
            .FirstOrDefaultAsync(x => x.ResetToken == resetToken && x.ResetTokenExpiry > DateTime.UtcNow);
    }

    public async Task RemoveResetTokenAsync(Guid resetToken)
    {
        var identity = await _db.MasterIdentities.FirstOrDefaultAsync(x => x.ResetToken == resetToken);
        if (identity is not null)
        {
            identity.ResetToken = null;
            identity.ResetTokenExpiry = null;
        }
    }

    public Task<bool> MobileExistsAsync(string mobile, CancellationToken cancellationToken = default)
    {
        return _db.MasterIdentities.AnyAsync(x => x.Mobile == mobile, cancellationToken);
    }

    public Task AddIdentityAsync(MasterIdentity identity, CancellationToken cancellationToken = default)
    {
        return _db.MasterIdentities.AddAsync(identity, cancellationToken).AsTask();
    }

    public Task<MasterIdentity?> GetIdentityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.MasterIdentities
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<MasterIdentity?> GetIdentityByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return _db.MasterIdentities
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task StoreRefreshTokenAsync(Guid identityId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var identity = await _db.MasterIdentities.FirstOrDefaultAsync(x => x.Id == identityId, cancellationToken);
        if (identity is null)
            throw new InvalidOperationException($"شناسه یافت نشد: {identityId}");

        identity.RefreshToken = refreshToken;
    }

    public async Task InvalidateRefreshTokenAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        var identity = await _db.MasterIdentities.FirstOrDefaultAsync(x => x.Id == identityId, cancellationToken);
        if (identity != null)
        {
            identity.RefreshToken = null;
        }
    }

    public Task<bool> UserExistsByMobileAsync(string mobile)
    {
        return _db.Users.AnyAsync(u => u.MasterIdentity.Mobile == mobile);
    }
}