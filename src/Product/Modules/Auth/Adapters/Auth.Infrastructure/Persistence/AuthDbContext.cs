using System.Security.Cryptography;
using Auth.Domain.Entities;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<AppIdentityUser, IdentityRole<long>, long>(options)
{
    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();

    public override int SaveChanges()
    {
        StampRefreshSessionRowVersions();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampRefreshSessionRowVersions();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }

    private void StampRefreshSessionRowVersions()
    {
        foreach (var entry in ChangeTracker.Entries<RefreshSession>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Property(session => session.RowVersion).CurrentValue = RandomNumberGenerator.GetBytes(8);
            }
        }
    }
}
