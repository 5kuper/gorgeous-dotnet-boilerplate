using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

internal sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).ValueGeneratedOnAdd();
        builder.Property(user => user.PublicId).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(320);
        builder.Property(user => user.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(user => user.RegistrationMethod).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(user => user.CreatedAtUtc).IsRequired();
        builder.Property(user => user.UpdatedAtUtc).IsRequired();

        builder.HasIndex(user => user.PublicId).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Navigation(user => user.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(user => user.Roles)
            .WithOne()
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
