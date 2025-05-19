namespace UserManagement.API.Endpoints.Users.DeactivateUser
{
    public record DeactivateUserCommand(Guid Id) : ICommand;
}
