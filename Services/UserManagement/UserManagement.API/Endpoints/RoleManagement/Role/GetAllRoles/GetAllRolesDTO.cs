﻿namespace UserManagement.API.Endpoints.RoleManagement.Role.GetAllRoles;

public record GetAllRolesQuery() : IQuery<GetAllRolesResult>;

public record GetAllRolesResult(IEnumerable<RoleDto> Roles);

public record RoleDto(int Id, string Name);
