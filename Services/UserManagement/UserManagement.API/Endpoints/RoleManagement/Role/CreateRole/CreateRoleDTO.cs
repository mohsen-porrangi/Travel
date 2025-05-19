namespace UserManagement.API.Endpoints.RoleManagement.Role.CreateRole;

public record CreateRoleCommand(string Name) : ICommand<int>;
