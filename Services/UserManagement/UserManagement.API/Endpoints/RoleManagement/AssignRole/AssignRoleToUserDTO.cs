namespace UserManagement.API.Endpoints.RoleManagement.AssignRole;

public record AssignRoleToUserCommand(Guid UserId, int RoleId) : ICommand;