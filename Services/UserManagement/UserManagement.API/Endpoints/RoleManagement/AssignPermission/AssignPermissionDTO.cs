namespace UserManagement.API.Endpoints.RoleManagement.AssignPermission;

public record AssignPermissionCommand(int RoleId, int PermissionId) : ICommand;
