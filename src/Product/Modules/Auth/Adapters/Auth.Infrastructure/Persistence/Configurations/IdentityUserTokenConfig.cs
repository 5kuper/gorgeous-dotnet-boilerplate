using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class IdentityUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<long>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<long>> builder)
    {
        builder.ToTable("identity_user_tokens");
    }
}
