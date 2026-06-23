using Auth.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class AppIdentityUserConfig : IEntityTypeConfiguration<AppIdentityUser>
{
    public void Configure(EntityTypeBuilder<AppIdentityUser> builder)
    {
        builder.ToTable("identity_users");
        builder.HasIndex(user => user.UserId).IsUnique();
        builder.HasIndex(user => user.UserPublicId).IsUnique();
    }
}
