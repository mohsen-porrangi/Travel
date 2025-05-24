// فایل: Services/UserManagement/UserManagement.API/Endpoints/Users/UserStatus/ChangeUserStatusDto.cs
namespace UserManagement.API.Endpoints.Admin.UserStatus;

public record ChangeUserStatusCommand(Guid Id, bool IsActive) : ICommand;