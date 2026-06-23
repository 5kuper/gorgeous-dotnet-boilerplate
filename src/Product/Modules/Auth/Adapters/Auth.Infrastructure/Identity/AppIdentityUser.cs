using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Identity;

public sealed class AppIdentityUser : IdentityUser<long>
{
    public long UserId { get; set; }

    public Guid UserPublicId { get; set; }
}
