﻿namespace UserManagement.API.Endpoints.RoleManagement.GetUserRoles;
public record GetUserRolesQuery(Guid UserId) : IQuery<GetUserRolesResult>;

public record GetUserRolesResult(IEnumerable<UserRoleDto> Roles);

public record UserRoleDto(
    int RoleId,
    string RoleName,
    DateTime AssignedAt
);