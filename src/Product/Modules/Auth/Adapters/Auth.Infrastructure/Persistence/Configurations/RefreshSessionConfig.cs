using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

internal sealed class RefreshSessionConfig : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        builder.ToTable("refresh_sessions");
        builder.HasKey(session => session.Id);

        builder.Property(session => session.Id).ValueGeneratedOnAdd();
        builder.Property(session => session.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(session => session.DeviceName).HasMaxLength(200);
        builder.Property(session => session.IpAddress).HasMaxLength(128);
        builder.Property(session => session.ExpiresAtUtc).IsRequired();
        builder.Property(session => session.CreatedAtUtc).IsRequired();
        builder.Property(session => session.RowVersion)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasIndex(session => session.TokenHash).IsUnique();
        builder.HasIndex(session => session.UserId);
    }
}
