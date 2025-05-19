namespace UserManagement.API.Endpoints.Auth.Password.ResetPassword;

public record ResetPasswordCommand(Guid ResetToken, string NewPassword) : ICommand;