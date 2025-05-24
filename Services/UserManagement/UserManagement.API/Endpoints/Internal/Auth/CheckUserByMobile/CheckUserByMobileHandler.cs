﻿using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Auth
{
    internal sealed class CheckUserByMobileHandler : IQueryHandler<CheckUserByMobileQuery, CheckUserByMobileResponse>
    {
        private readonly AppDbContext _db;

        public CheckUserByMobileHandler(AppDbContext db) => _db = db;

        public async Task<CheckUserByMobileResponse> Handle(CheckUserByMobileQuery request, CancellationToken cancellationToken)
        {
            var exists = await _db.Users.AnyAsync(u => u.MasterIdentity.Mobile == request.Mobile, cancellationToken);
            return new CheckUserByMobileResponse(exists);
        }
    }

}
