using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class IdentityUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<long>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<long>> builder)
    {
        builder.ToTable("identity_user_roles");
    }
}
