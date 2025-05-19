namespace UserManagement.API.Endpoints.RoleManagement.UnassignPermission;

public record UnassignPermissionCommand(int RoleId, int PermissionId) : ICommand;
