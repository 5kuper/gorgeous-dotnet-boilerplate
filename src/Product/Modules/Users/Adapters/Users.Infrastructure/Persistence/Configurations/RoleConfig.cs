using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id).ValueGeneratedOnAdd();
        builder.Property(role => role.Code).HasMaxLength(128).IsRequired();
        builder.Property(role => role.Name).HasMaxLength(200).IsRequired();
        builder.Property(role => role.Description).HasMaxLength(500);
        builder.Property(role => role.CreatedAtUtc).IsRequired();
        builder.Property(role => role.UpdatedAtUtc).IsRequired();

        builder.HasIndex(role => role.Code).IsUnique();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new
            {
                Id = 1L,
                Code = "user",
                Name = "User",
                Description = "Default registered user.",
                IsSystem = true,
                CreatedAtUtc = seedDate,
                UpdatedAtUtc = seedDate
            },
            new
            {
                Id = 2L,
                Code = "admin",
                Name = "Administrator",
                Description = "System administrator.",
                IsSystem = true,
                CreatedAtUtc = seedDate,
                UpdatedAtUtc = seedDate
            },
            new
            {
                Id = 3L,
                Code = "support",
                Name = "Support",
                Description = "Support operator.",
                IsSystem = true,
                CreatedAtUtc = seedDate,
                UpdatedAtUtc = seedDate
            });
    }
}
