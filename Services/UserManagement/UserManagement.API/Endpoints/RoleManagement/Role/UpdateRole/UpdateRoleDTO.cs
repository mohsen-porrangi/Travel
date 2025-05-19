namespace UserManagement.API.Endpoints.RoleManagement.Role.UpdateRole;

public record UpdateRoleCommand(int Id, string Name) : ICommand;
