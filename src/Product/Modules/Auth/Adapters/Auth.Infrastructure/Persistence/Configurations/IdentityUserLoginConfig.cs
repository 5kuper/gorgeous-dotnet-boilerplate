using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class IdentityUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<long>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<long>> builder)
    {
        builder.ToTable("identity_user_logins");
    }
}
