namespace UserManagement.API.Endpoints.Users.GetUserById
{
    public record GetUserByIdQuery(Guid Id) : IQuery<GetUserByIdResult>;

    public record GetUserByIdResult(
        Guid Id,
        string Name,
        string Family,
        string Email,
        bool IsActive
    );

}
