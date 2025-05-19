namespace UserManagement.API.Endpoints.Auth.Password.ForgotPassword;

public record ForgotPasswordCommand(string Mobile) : ICommand;
