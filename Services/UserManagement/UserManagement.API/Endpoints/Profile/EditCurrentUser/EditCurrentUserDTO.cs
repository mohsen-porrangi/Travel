using BuildingBlocks.Enums;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser
{
    public record EditCurrentUserCommand(
     Guid IdentityId,
     string Name,
     string Family,
     string? NationalCode,
     Gender? Gender,
     DateTime BirthDate
 ) : ICommand;
}
