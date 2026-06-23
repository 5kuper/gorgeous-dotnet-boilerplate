using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.BuildingBlocks.Application.Abstractions;

namespace Shared.WebFramework;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? PublicUserId
    {
        get
        {
            string? value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal?.FindFirstValue("sub");

            return Guid.TryParse(value, out var publicUserId) ? publicUserId : null;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
}
