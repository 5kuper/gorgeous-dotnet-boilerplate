using Microsoft.EntityFrameworkCore;
using Shared.BuildingBlocks.Application.Abstractions;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken ct)
    {
        await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}

