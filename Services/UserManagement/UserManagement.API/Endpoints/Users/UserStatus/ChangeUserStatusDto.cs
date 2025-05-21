// فایل: Services/UserManagement/UserManagement.API/Endpoints/Users/UserStatus/ChangeUserStatusDto.cs
namespace UserManagement.API.Endpoints.Users.UserStatus;

public record ChangeUserStatusCommand(Guid Id, bool IsActive) : ICommand;