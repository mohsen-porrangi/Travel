﻿using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.GetUserRoles;
internal sealed class GetUserRolesQueryHandler(
    AppDbContext db,
    IUserRepository userRepository,
    ILogger<GetUserRolesQueryHandler> logger
) : IQueryHandler<GetUserRolesQuery, GetUserRolesResult>
{
    public async Task<GetUserRolesResult> Handle(GetUserRolesQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting roles for user {UserId}", query.UserId);

        // بررسی وجود کاربر
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found: {UserId}", query.UserId);
            throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه {query.UserId} یافت نشد");
        }

        // دریافت نقش‌های کاربر
        var userRoles = await db.UserRoles
            .Where(ur => ur.UserId == query.UserId)
            .Include(ur => ur.Role)
            .Select(ur => new UserRoleDto(
                ur.RoleId,
                ur.Role.Name,
                ur.AssignedAt
            ))
            .OrderBy(ur => ur.RoleName)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} roles for user {UserId}", userRoles.Count, query.UserId);

        return new GetUserRolesResult(userRoles);
    }
}