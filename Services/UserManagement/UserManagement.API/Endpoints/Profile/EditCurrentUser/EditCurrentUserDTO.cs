// فایل: Services/UserManagement/UserManagement.API/Endpoints/Profile/EditCurrentUser/EditCurrentUserDTO.cs
using BuildingBlocks.Enums;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser
{
    public record EditCurrentUserCommand(
        Guid IdentityId,
        string Name,
        string Family,
        string? NationalCode,
        Gender? Gender,
        DateTime BirthDate,
        // فیلدهای اختیاری برای تغییر رمز عبور
        string? CurrentPassword = null,
        string? NewPassword = null
    ) : ICommand;
}