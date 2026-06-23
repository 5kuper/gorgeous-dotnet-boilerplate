using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class IdentityRoleClaimConfig : IEntityTypeConfiguration<IdentityRoleClaim<long>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<long>> builder)
    {
        builder.ToTable("identity_role_claims");
    }
}
