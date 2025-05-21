// فایل: Services/UserManagement/UserManagement.API/Endpoints/Users/UserStatus/ChangeUserStatusEndpoint.cs
using UserManagement.API.Endpoints.AccessControl.Permissions;

namespace UserManagement.API.Endpoints.Users.UserStatus;

public class ChangeUserStatusEndpoint : ICarterModule
{
    [RequirePermission(UserPermissions.Activate)] // استفاده از مجوز مناسب که هر دو عملیات را پوشش دهد
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{id:guid}/status", async (
            Guid id,
            ChangeUserStatusCommand body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = body with { Id = id };
            await sender.Send(command, ct);
            return Results.NoContent();
        })
.WithName("ToggleUserStatus")
.WithDescription("تغییر وضعیت فعال/غیرفعال کاربر")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags("Users")
            .RequireAuthorization("Admin");
    }
}