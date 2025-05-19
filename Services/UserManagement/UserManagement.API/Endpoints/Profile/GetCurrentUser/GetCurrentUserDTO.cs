namespace UserManagement.API.Endpoints.Profile.GetCurrentUser
{

    public record GetCurrentUserQuery(Guid IdentityId) : IQuery<GetCurrentUserResult>;

    public record GetCurrentUserResult(
        Guid Id,
        string Name,
        string Family,
        string Email,
        string? Mobile,
        bool IsActive
    );
}
