namespace UserManagement.API.Endpoints.Auth.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword, Guid IdentityId) : ICommand;
