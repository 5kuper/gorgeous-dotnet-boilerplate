using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class IdentityUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<long>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<long>> builder)
    {
        builder.ToTable("identity_user_claims");
    }
}
