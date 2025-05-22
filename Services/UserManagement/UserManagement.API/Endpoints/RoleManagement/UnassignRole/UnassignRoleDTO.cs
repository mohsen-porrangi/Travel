namespace UserManagement.API.Endpoints.RoleManagement.UnassignRole;
public record UnassignRoleFromUserCommand(Guid UserId, int RoleId) : ICommand;

