using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using UserManagement.API.Infrastructure.Data.Models;

namespace UserManagement.API.Infrastructure.Data.Models.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Module).IsRequired();
            builder.Property(x => x.Action).IsRequired();
        }
    }
}
