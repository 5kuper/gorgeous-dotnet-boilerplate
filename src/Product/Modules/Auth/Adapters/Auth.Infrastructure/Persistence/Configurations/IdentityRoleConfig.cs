using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class IdentityRoleConfig : IEntityTypeConfiguration<IdentityRole<long>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<long>> builder)
    {
        builder.ToTable("identity_roles");
    }
}
