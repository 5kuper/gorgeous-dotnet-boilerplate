using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

internal sealed class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(userRole => userRole.Id);

        builder.Property(userRole => userRole.Id).ValueGeneratedOnAdd();
        builder.Property(userRole => userRole.AssignedAtUtc).IsRequired();

        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId }).IsUnique();

        builder.HasOne(userRole => userRole.Role)
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
