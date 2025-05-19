namespace UserManagement.API.Endpoints.Users.ActivateUser
{
    public record ActivateUserCommand(Guid Id) : ICommand;
    public record ActiveUserResult(bool IsSucces);
}
