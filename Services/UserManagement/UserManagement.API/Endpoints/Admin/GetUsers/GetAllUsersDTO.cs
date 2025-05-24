namespace UserManagement.API.Endpoints.Admin.GetUsers;
public record GetAllUsersQuery() : IQuery<GetAllUsersResult>;

public record GetAllUsersResult(IEnumerable<UserDto> Users);

public record UserDto(
    Guid Id,
    string Name,
    string Family,
    string Email,
    bool IsActive
);