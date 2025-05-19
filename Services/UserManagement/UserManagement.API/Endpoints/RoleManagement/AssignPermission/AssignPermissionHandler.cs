﻿using BuildingBlocks.Contracts;
using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.RoleManagement.AssignPermission;

internal sealed class AssignPermissionCommandHandler(AppDbContext db, IUnitOfWork uow)
    : ICommandHandler<AssignPermissionCommand>
{
    public async Task<Unit> Handle(AssignPermissionCommand command, CancellationToken cancellationToken)
    {
        var exists = await db.RolePermissions.AnyAsync(rp =>
            rp.RoleId == command.RoleId && rp.PermissionId == command.PermissionId,
            cancellationToken);

        if (!exists)
        {
            await db.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = command.RoleId,
                PermissionId = command.PermissionId
            }, cancellationToken);

            await uow.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
