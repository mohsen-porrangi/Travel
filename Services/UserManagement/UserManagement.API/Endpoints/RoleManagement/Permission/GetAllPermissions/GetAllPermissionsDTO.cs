﻿namespace UserManagement.API.Endpoints.RoleManagement.Permission.GetAllPermissions;

public record GetAllPermissionsQuery() : IQuery<GetAllPermissionsResult>;

public record GetAllPermissionsResult(IEnumerable<PermissionDto> Permissions);

public record PermissionDto(int Id, string Module, string Action, string Code, string Description);
