namespace UserManagement.API.Endpoints.Auth.Login;

public record LoginUserCommand(
   // string? Email,
    string? Mobile,
    string? Password,
    string? Otp
) : ICommand<string>;
